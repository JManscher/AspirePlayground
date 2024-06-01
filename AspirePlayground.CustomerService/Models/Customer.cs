using AspirePlayground.IntegrationEvents.CustomerEvents;

namespace AspirePlayground.CustomerService.Models;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Company { get; set; }
    public string Title { get; set; }
    public DateTime ModifiedDateUtc { get; set; }

    private void ApplyCreate(CustomerEvent customerCreated)
    {
        CustomerId = Guid.Parse(customerCreated.CustomerId);
        Name = customerCreated.Name;
        Email = customerCreated.Email;
        PhoneNumber = customerCreated.PhoneNumber;
        Address = customerCreated.Address;
        Company = customerCreated.Company;
        Title = customerCreated.Title;
        ModifiedDateUtc = customerCreated.CreatedAtUtc;
    }
    
    private void ApplyUpdate(CustomerEvent customerUpdated)
    {
        Name = customerUpdated.Name;
        Email = customerUpdated.Email;
        PhoneNumber = customerUpdated.PhoneNumber;
        Address = customerUpdated.Address;
        Company = customerUpdated.Company;
        Title = customerUpdated.Title;
        ModifiedDateUtc = customerUpdated.CreatedAtUtc;
    }
    
    public void Apply(CustomerEvent @event, ILogger logger)
    {
        logger.LogDebug("Applying event {event} to customer {CustomerId}", @event.EventType, @event.CustomerId);
        switch (@event.EventType)
        {
            case CustomerEventTypeEnum.Created:
                ApplyCreate(@event);
                break;
            case CustomerEventTypeEnum.Updated:
                ApplyUpdate(@event);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }
    
}