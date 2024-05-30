using AspirePlayground.CustomerService.Models;
using AspirePlayground.CustomerService.Repository;
using Dapr.Client;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspirePlayground.CustomerService.Delegates;

public static class CustomerDelegates
{
    public static async Task<Results<Ok<Customer>, NoContent>> GetCustomerById([FromServices]ILogger<ICustomerRepository> logger, [FromServices]ICustomerRepository customerRepository, [FromRoute]Guid id)
    {
        logger.LogInformation("Customer with id {id} was requested", id);
        var customer = await customerRepository.GetCustomerById(id);
        if(customer is null)
        {
            logger.LogInformation("Customer with id {id} was not found", id);
            return TypedResults.NoContent();
        }
        logger.LogInformation("Customer with id {id} was found: {customer}", id, customer);
        return TypedResults.Ok(customer);
    }

    public static async Task<Ok> RepublishCustomerEvents(
        [FromServices]ILogger<ICustomerRepository> logger,
        [FromServices]DaprClient daprClient,
        [FromServices]ICustomerRepository customerRepository,
        [FromBody]RepublishRequest request)
    {
        logger.LogInformation("Republishing customer events");
        var metaData = new Dictionary<string, string>
        {
            { "republished", DateTimeOffset.UtcNow.ToString("O") },
            { "republishId",  request.RequestId.ToString()}
        };
        await foreach(var @event in customerRepository.ReadAllEvents())
        {
            await daprClient.PublishEventAsync("pubsub", "customer-events", @event, metaData);
        }
        logger.LogInformation("Customer events were republished");
        return TypedResults.Ok();
    }
}

public class RepublishRequest
{
    public Guid RequestId {get; set;}
}