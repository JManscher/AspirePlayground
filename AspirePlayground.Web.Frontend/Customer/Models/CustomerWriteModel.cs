using System.ComponentModel.DataAnnotations;

namespace AspirePlayground.Web.Frontend.Customer.Models;

public class CustomerWriteModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}