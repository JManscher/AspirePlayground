using System.Net;
using System.Text.Json;
using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.Web.Backend.Customers.Models;
using Dapr.Client;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Core;

namespace AspirePlayground.Web.Backend.Customers;

public class CustomerService(DaprClient daprClient, CosmosClient cosmosClient, ILogger<ICustomerService> logger) : ICustomerService
{
    private readonly Lazy<Task<Container>> _container = new Lazy<Task<Container>>(async () =>
    {

        var db = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        var container = await db.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
        return container;
    });

    private readonly static string containerName = "customers";
    private readonly static string databaseName = "bff-db";

    public async Task<Customer?> GetCustomerById(Guid id)
    {
        var response = await  daprClient.InvokeMethodWithResponseAsync(daprClient.CreateInvokeMethodRequest(HttpMethod.Get, "customerservice", $"customer/{id}"));
        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        var customer = await response.Content.ReadFromJsonAsync<Customer>();
        return customer;
        
    }

    public async Task<Guid> CreateCustomer(CustomerWriteModel customerWriteModel)
    {
        var guid = Guid.NewGuid();
        await daprClient.PublishEventAsync("pubsub", "customer-events", new CustomerEvent()
        {
            EventType = CustomerEventTypeEnum.Created,
            CustomerId = guid.ToString(),
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
            CustomerId = id.ToString(),
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

    public async Task StoreCachedCustomer(CustomerChangedEvent customerChangedEvent)
    {
        var container = await _container.Value;
        var response = await container.ReadItemStreamAsync(customerChangedEvent.CustomerId, new PartitionKey(customerChangedEvent.CustomerId));
        if(response.StatusCode == HttpStatusCode.OK)
        {
            var existing = await JsonSerializer.DeserializeAsync<Customer>(response.Content);
            if(existing?.ModifiedDateUtc > customerChangedEvent.CreatedAtUtc)
            {
                logger.LogInformation("New customer event {existingStreamId} already stored, ignoring {newCustomerStreamId}", existing.Id, customerChangedEvent.CustomerId);
                return;
            }
        }
        var customer = new Customer(
            Guid.Parse(customerChangedEvent.CustomerId),
            customerChangedEvent.Name,
            customerChangedEvent.Email,
            customerChangedEvent.PhoneNumber,
            customerChangedEvent.Address,
            customerChangedEvent.Company,
            customerChangedEvent.Title,
            customerChangedEvent.CreatedAtUtc
        );
        logger.LogInformation("Storing customer {@customer}", JsonSerializer.Serialize(customer));
        await container.UpsertItemAsync(customer, new PartitionKey(customer.Id.ToString()));
    }
}
