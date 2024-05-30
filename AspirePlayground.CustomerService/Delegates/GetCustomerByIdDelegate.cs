using AspirePlayground.CustomerService.Models;
using AspirePlayground.CustomerService.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspirePlayground.CustomerService.Delegates;

public static class GetCustomerByIdDelegate
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

    public static async Task<Ok<List<Customer>>> GetCustomers([FromServices]ICustomerRepository customerRepository)
    {
        return TypedResults.Ok(await customerRepository.GetCustomers());
    }
}