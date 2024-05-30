using AspirePlayground.Web.Frontend.Customer.Models;
using Dapr.Client;

namespace AspirePlayground.Web.Frontend.Customer;

public class CustomerApiClient(DaprClient daprClient)
{
    public Task<Models.Customer> GetCustomerById(Guid id)
    {
        return daprClient.InvokeMethodAsync<Models.Customer>(HttpMethod.Get, "bff", $"customer/{id}");
    }
    
    public Task<Guid> CreateCustomer(CustomerWriteModel customer)
    {
        return daprClient.InvokeMethodAsync<CustomerWriteModel, Guid>(HttpMethod.Post, "bff", $"customer", customer);
    }

    public Task UpdateCustomer(CustomerWriteModel customer, string? id)
    {
        return daprClient.InvokeMethodAsync(HttpMethod.Put, "bff", $"customer/{id}", customer);

    }

    public Task<List<Models.Customer>> GetCustomers()
    {
        return daprClient.InvokeMethodAsync<List<Models.Customer>>(HttpMethod.Get, "bff", $"customer");
    }
}