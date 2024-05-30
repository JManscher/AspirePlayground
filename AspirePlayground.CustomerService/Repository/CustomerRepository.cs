using AspirePlayground.CustomerService.Models;
using AspirePlayground.IntegrationEvents.CustomerEvents;
using Microsoft.Azure.Cosmos;

namespace AspirePlayground.CustomerService.Repository;

public class CustomerRepository: ICustomerRepository
{
    private readonly ILogger<CustomerRepository> _logger;

    //private readonly CosmosClient _cosmosClient;
    private readonly Lazy<Task<Container>> _lazyEventContainer;
    //private readonly Lazy<Task<Container>> _lazyCustomerViewContainer;
                                                             
     public CustomerRepository(CosmosClient client, ILogger<CustomerRepository> logger)
     {
         _logger = logger;

         _lazyEventContainer = new Lazy<Task<Container>>(async () =>
         {
     
             var db = await client.CreateDatabaseIfNotExistsAsync("AspirePlayground");
             var container = await db.Database.CreateContainerIfNotExistsAsync("CustomerEvents", "/CustomerId");
 
             return container.Container;
         });
     }
     public async Task<Customer?> GetCustomerById(Guid id)
    {
        var container = await _lazyEventContainer.Value;
        
        var query = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @id ORDER BY c.Sk ASC")
            .WithParameter("@id", id);
        
        var iterator = container.GetItemQueryIterator<CustomerEvent>(query);
        
        var customer = new Customer();

        while(iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync();
            
            foreach(var result in results)
            {
                customer.Apply(result, _logger);
            }
        }

        return customer;
    }

    public async Task AppendEvent(CustomerEvent @event) 
    {
        var container = await _lazyEventContainer.Value;
        await container.CreateItemAsync(@event, new PartitionKey(@event.Id));
    }

    public async Task<List<Customer>> GetCustomers()
    {
        var container = await _lazyEventContainer.Value;

        var query = new QueryDefinition("SELECT * FROM c WHERE c.EventType = 'Created' ORDER BY c.CreatedAtUtc DESC");
    }
}