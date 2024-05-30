namespace AspirePlayground.Web.Backend.Customers.Models;

public record Customer(
    Guid Id, 
    string Name, 
    string Email, 
    string PhoneNumber, 
    string Address, 
    string Company, 
    string Title
);