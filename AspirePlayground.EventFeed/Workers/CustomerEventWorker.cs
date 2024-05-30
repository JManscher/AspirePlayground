using System.Collections.Immutable;
using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.ServiceDefaults.Meters;
using Dapr.Client;
using Microsoft.Azure.Cosmos;

namespace AspirePlayground.EventFeed;

public class CustomerEventWorker(ILogger<CustomerEventWorker> logger, DaprClient daprClient, CosmosClient cosmosClient, CustomerMeter meter)
    : BackgroundService
{
   
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync("AspirePlayground")).Database;
        var leaseContainer = await database.CreateContainerIfNotExistsAsync("CustomerEventsLeases", "/id", cancellationToken: stoppingToken);
        var container = (await database.CreateContainerIfNotExistsAsync("CustomerEvents", "/CustomerId", cancellationToken: stoppingToken)).Container;

        ChangeFeedProcessor changeFeedProcessor = container
            .GetChangeFeedProcessorBuilder<CustomerEvent>(processorName: nameof(CustomerEventWorker),
                onChangesDelegate:  HandleChangesAsync)
            .WithInstanceName(nameof(AspirePlayground.EventFeed))
            .WithLeaseContainer(leaseContainer)
            .Build();
        
        logger.LogInformation("Starting change feed processor for {containerId}", container.Id);
        await changeFeedProcessor.StartAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("{worker} running at: {time}", nameof(CustomerEventWorker), DateTimeOffset.Now);
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("{worker} Stopping change feed processor for {containerId}", nameof(CustomerEventWorker), container.Id);
                await changeFeedProcessor.StopAsync();
                return;
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<CustomerEvent> changes, CancellationToken cancellationtoken)
    {
        logger.LogInformation("Started handling changes for lease {leaseToken}...", context.LeaseToken);
        logger.LogInformation("Change Feed request consumed {requestCharge} RU.", context.Headers.RequestCharge);

        // We may want to track any operation's Diagnostics that took longer than some threshold
        if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
        {
            logger.LogInformation("Change Feed request took {elapsedTime} longer than expected. Diagnostics: {diagnostics}", context.Diagnostics.GetClientElapsedTime(), context.Diagnostics);
        }
        
        foreach (CustomerEvent item in changes)
        {
            logger.LogDebug("Detected operation for item with id {id}, created at {creationTime}.", item.Id, item.CreatedAtUtc);
        }
        
        var customerChangedEvents = changes.Select(c => new CustomerChangedEvent
        {
            Address = c.Address,
            Company = c.Company,
            Email = c.Email,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            Title = c.Title,
            EventType = c.EventType,
            CustomerId = c.Id
        }).ToImmutableList();

        var result = await daprClient.BulkPublishEventAsync<CustomerChangedEvent>(
            "pubsub",
            "customer-changed", 
            customerChangedEvents, cancellationToken: cancellationtoken);
        
        meter.CustomerChangedEventCounter.Add(customerChangedEvents.Count);
        
        if(result.FailedEntries.Any())
        {
            logger.LogError("Failed to publish {failed} events to pubsub out of {totalCount}", result.FailedEntries.Count, customerChangedEvents.Count - result.FailedEntries.Count);
        }
        
        logger.LogInformation("Published {count} events to pubsub", customerChangedEvents.Count - result.FailedEntries.Count);
        
    }
}