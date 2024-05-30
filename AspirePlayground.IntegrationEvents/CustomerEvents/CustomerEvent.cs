using Newtonsoft.Json;

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
    
    [JsonProperty("CustomerId")]
    public required string Id { get; init; }
    
    public DateTime CreatedAtUtc { get; set; }

    public long Sk => CreatedAtUtc.Ticks;
    
    [JsonProperty("id")]
    public string StreamId => $"{Id}_{Sk}";
}

public enum CustomerEventTypeEnum
{
    Created,
    Updated
}



