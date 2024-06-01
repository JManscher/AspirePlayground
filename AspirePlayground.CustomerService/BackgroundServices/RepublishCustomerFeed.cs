using System.Runtime.CompilerServices;
using System.Threading.Channels;
using AspirePlayground.CustomerService.Model;
using AspirePlayground.IntegrationEvents.CustomerEvents;
using Dapr.Client;
using Microsoft.Azure.Cosmos;
public class RepublishCustomerFeed : BackgroundService
{
    private readonly Channel<RepublishCustomerEvents> _queue;
    private readonly ILogger<RepublishCustomerFeed> _logger;
    private readonly DaprClient _daprClient;

    private readonly Lazy<Task<Container>> _lazyEventContainer;
    public RepublishCustomerFeed(Channel<RepublishCustomerEvents> channel, ILogger<RepublishCustomerFeed> logger, CosmosClient cosmosClient, DaprClient daprClient)
    {
        _queue = channel;
        _logger = logger;
        _daprClient = daprClient;
        _lazyEventContainer = new Lazy<Task<Container>>(async () =>
        {
            var db = await cosmosClient.CreateDatabaseIfNotExistsAsync("AspirePlayground");
            var container = await db.Database.CreateContainerIfNotExistsAsync("CustomerEvents", "/CustomerId");
            return container.Container;
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Republishing customer events");
            var metaData = new Dictionary<string, string>
            {
                { "republished", DateTimeOffset.UtcNow.ToString("O") },
                { "republishId",  item.RequestId.ToString()}
            };
            await foreach (var @event in ReadAllEvents(stoppingToken))
            {
                await _daprClient.PublishEventAsync("pubsub", "customer-changed", new CustomerChangedEvent{
                    Address = @event.Address,
                    Company = @event.Company,
                    CreatedAtUtc = @event.CreatedAtUtc,
                    CustomerId = @event.CustomerId,
                    Email = @event.Email,
                    EventType = @event.EventType,
                    Name = @event.Name,
                    PhoneNumber = @event.PhoneNumber,
                    Title = @event.Title
                
                }, metaData, stoppingToken);
            }
            _logger.LogInformation("Customer events were republished");

        }
    }

    private async IAsyncEnumerable<CustomerEvent> ReadAllEvents([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var container = await _lazyEventContainer.Value;
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.Sk ASC");
        var iterator = container.GetItemQueryIterator<CustomerEvent>(query);
        while(iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync(stoppingToken);
            foreach(var result in results)
            {
                yield return result;
            }
        }
    }
}
