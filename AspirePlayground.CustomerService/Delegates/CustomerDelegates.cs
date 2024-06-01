using System.Threading.Channels;
using AspirePlayground.CustomerService.Model;
using AspirePlayground.CustomerService.Models;
using AspirePlayground.CustomerService.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspirePlayground.CustomerService.Delegates;

public static class CustomerDelegates
{
    public static async Task<Results<Ok<Customer>, NoContent>> GetCustomerById([FromServices] ILogger<ICustomerRepository> logger, [FromServices] ICustomerRepository customerRepository, [FromRoute] Guid id)
    {
        logger.LogInformation("Customer with id {id} was requested", id);
        var customer = await customerRepository.GetCustomerById(id);
        if (customer is null)
        {
            logger.LogInformation("Customer with id {id} was not found", id);
            return TypedResults.NoContent();
        }
        logger.LogInformation("Customer with id {id} was found: {customer}", id, customer);
        return TypedResults.Ok(customer);
    }

    public static Results<Ok, StatusCodeHttpResult> RepublishCustomerEvents(
        [FromServices] Channel<RepublishCustomerEvents> queue,
        [FromServices] ILogger<RepublishCustomerFeed> logger,
        [FromBody] RepublishRequest request)
    {
        try
        {
            queue.Writer.TryWrite(new RepublishCustomerEvents { RequestId = request.RequestId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to republish customer events for {request}", request);
            return TypedResults.StatusCode(500);
        }
        return TypedResults.Ok();
    }
}
