using System.Text.Json;
using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.Web.Backend.Customers.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspirePlayground.Web.Backend.Customers.Delegates;

public static class Customers
{
    public static async Task<Results<Ok<Customer>, NoContent, BadRequest<ProblemDetails>>> GetCustomerById(
        ICustomerService customerService, Guid id)
    {
        var customer = await customerService.GetCustomerById(id);
        return customer is not null
            ? TypedResults.Ok(customer)
            : TypedResults.NoContent();
    }

    public static async Task<Ok<Guid>> CreateCustomer([FromServices] ILogger<ICustomerService> logger, [FromServices] ICustomerService customerService,
        [FromBody] CustomerWriteModel customerWriteModel)
    {
        logger.LogInformation("Creating customer {@customerWriteModel}", JsonSerializer.Serialize(customerWriteModel));
        var result = await customerService.CreateCustomer(customerWriteModel);
        logger.LogInformation("Customer {@customerWriteModel} was created with id {id}", customerWriteModel, result);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok> UpdateCustomer([FromServices] ILogger<ICustomerService> logger, [FromServices] ICustomerService customerService,
        [FromBody] CustomerWriteModel customerWriteModel, [FromRoute] Guid id)
    {
        logger.LogInformation("Creating customer {@customerWriteModel}", JsonSerializer.Serialize(customerWriteModel));
        await customerService.UpdateCustomer(customerWriteModel, id);
        logger.LogInformation("Customer {@customerWriteModel} was updated with id {id}", customerWriteModel, id);
        return TypedResults.Ok();
    }

    public async static Task<Ok<List<Customer>>> GetCustomers([FromServices] ICustomerService customerService)
    {
        return TypedResults.Ok(await customerService.GetCustomers());
    }

}

