using Newtonsoft.Json;
// Have to use Newtonsoft.Json because Cosmos SDK does not support System.Text.Json

namespace AspirePlayground.IntegrationEvents.CustomerEvents;

public class CustomerEvent
{
    
    public CustomerEventTypeEnum EventType { get; init; }
    
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Address { get; init; }
    public required string Company { get; init; }
    public required string Title { get; init; }
    
    public required string CustomerId { get; init; }
    
    public DateTime CreatedAtUtc { get; set; }

    public long Sk => CreatedAtUtc.Ticks;
    
    [JsonProperty("id")]
    public string StreamId => $"{CustomerId}_{Sk}";
}

public enum CustomerEventTypeEnum
{
    Created,
    Updated
}



