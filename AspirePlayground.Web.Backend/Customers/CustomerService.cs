using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.Web.Backend.Customers.Models;
using Dapr.Client;
using Microsoft.Azure.Cosmos;

namespace AspirePlayground.Web.Backend.Customers;

public class CustomerService(DaprClient daprClient, CosmosClient cosmosClient, ILogger<ICustomerService> logger) : ICustomerService
{
    private readonly Lazy<Task<Container>> _container = new Lazy<Task<Container>>(async () =>
    {

        var db = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        var container = await db.Database.CreateContainerIfNotExistsAsync(containerName, "/CustomerId");
        return container;
    });

    private readonly static string containerName = "customers";
    private readonly static string databaseName = "bff-db";

    public Task<Customer?> GetCustomerById(Guid id)
    {
        return daprClient.InvokeMethodAsync<Customer?>(HttpMethod.Get, "customerservice", $"customer/{id}");
    }

    public async Task<Guid> CreateCustomer(CustomerWriteModel customerWriteModel)
    {
        var guid = Guid.NewGuid();
        await daprClient.PublishEventAsync("pubsub", "customer-events", new CustomerEvent()
        {
            EventType = CustomerEventTypeEnum.Created,
            Id = guid.ToString(),
            Name = customerWriteModel.Name,
            Email = customerWriteModel.Email,
            PhoneNumber = customerWriteModel.PhoneNumber,
            Address = customerWriteModel.Address,
            Company = customerWriteModel.Company,
            Title = customerWriteModel.Title,
            CreatedAtUtc = DateTime.UtcNow
        });

        return guid;
    }

    public Task UpdateCustomer(CustomerWriteModel customerWriteModel, Guid id)
    {
        return daprClient.PublishEventAsync("pubsub", "customer-events", new CustomerEvent()
        {
            EventType = CustomerEventTypeEnum.Updated,
            Id = id.ToString(),
            Name = customerWriteModel.Name,
            Email = customerWriteModel.Email,
            PhoneNumber = customerWriteModel.PhoneNumber,
            Address = customerWriteModel.Address,
            Company = customerWriteModel.Company,
            Title = customerWriteModel.Title,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    public async IAsyncEnumerable<Customer> GetCustomers()
    {
        var container = await _container.Value;

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<Customer>(query);


        while (iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync();

            foreach (var result in results)
            {
                yield return result;
            }
        }
    }

    public async Task StoreCachedCustomer(CustomerEvent customer)
    {
        var container = await _container.Value;
        var existing = await container.ReadItemAsync<CustomerEvent>(customer.Id.ToString(), new PartitionKey(customer.Id.ToString()));
        if (existing.Resource.Sk > customer.CreatedAtUtc.Ticks)
        {
            logger.LogInformation("New customer event {existingStreamId} already stored, ignoring {newCustomerStreamId}", existing.Resource.StreamId, customer.StreamId);
            return;
        }
        await container.UpsertItemAsync(customer);
    }
}
