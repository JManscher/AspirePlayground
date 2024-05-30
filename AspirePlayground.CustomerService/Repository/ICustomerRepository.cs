using AspirePlayground.CustomerService.Models;
using AspirePlayground.IntegrationEvents.CustomerEvents;

namespace AspirePlayground.CustomerService.Repository;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerById(Guid id);
    Task AppendEvent(CustomerEvent @event);
    IAsyncEnumerable<CustomerEvent> ReadAllEvents();

}