using Newtonsoft.Json;

namespace AspirePlayground.Web.Backend.Customers.Models;

public record Customer(
    [property: JsonProperty("id")]
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string Address,
    string Company,
    string Title,
    DateTime ModifiedDateUtc
);