namespace AspirePlayground.Web.Backend.Customers.Models;

public record CustomerWriteModel(
    string Name,
    string Email,
    string PhoneNumber,
    string Address,
    string Company,
    string Title)
{
    public string Name { get; set; } = Name;
    public string Email { get; set; } = Email;
    public string PhoneNumber { get; set; } = PhoneNumber;
    public string Address { get; set; } = Address;
    public string Company { get; set; } = Company;
    public string Title { get; set; } = Title;
}