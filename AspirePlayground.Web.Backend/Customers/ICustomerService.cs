using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.Web.Backend.Customers.Models;

namespace AspirePlayground.Web.Backend.Customers;

public interface ICustomerService
{
    Task<Customer?> GetCustomerById(Guid id);
    Task<Guid> CreateCustomer(CustomerWriteModel customerWriteModel);
    Task UpdateCustomer(CustomerWriteModel customerWriteModel, Guid id);
    IAsyncEnumerable<Customer> GetCustomers();
    Task StoreCachedCustomer(CustomerEvent customer);
}