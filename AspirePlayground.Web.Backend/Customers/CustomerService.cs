using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.Web.Backend.Customers.Models;
using Dapr.Client;

namespace AspirePlayground.Web.Backend.Customers;

public class CustomerService(DaprClient daprClient) : ICustomerService
{
    public Task<Customer?> GetCustomerById(Guid id)
    {
        return daprClient.InvokeMethodAsync<Customer?>( HttpMethod.Get,"customerservice", $"customer/{id}");
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

    public Task<List<Customer>> GetCustomers()
    {
        return daprClient.InvokeMethodAsync<List<Customer>>(HttpMethod.Get, "customerservice", "customer");
    }
}
