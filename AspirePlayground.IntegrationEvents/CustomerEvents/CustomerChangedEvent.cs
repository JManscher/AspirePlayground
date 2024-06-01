namespace AspirePlayground.IntegrationEvents.CustomerEvents;

public record CustomerChangedEvent
{
    public required CustomerEventTypeEnum EventType { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Address { get; init; }
    public required string Company { get; init; }
    public required string Title { get; init; }
    public required string CustomerId { get; init; }
    public required DateTime CreatedAtUtc { get; set; }

}