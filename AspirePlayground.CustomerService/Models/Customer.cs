using AspirePlayground.IntegrationEvents.CustomerEvents;

namespace AspirePlayground.CustomerService.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Company { get; set; }
    public string Title { get; set; }

    private void ApplyCreate(CustomerEvent customerCreated)
    {
        Id = Guid.Parse(customerCreated.Id);
        Name = customerCreated.Name;
        Email = customerCreated.Email;
        PhoneNumber = customerCreated.PhoneNumber;
        Address = customerCreated.Address;
        Company = customerCreated.Company;
        Title = customerCreated.Title;
    }
    
    private void ApplyUpdate(CustomerEvent customerUpdated)
    {
        Name = customerUpdated.Name;
        Email = customerUpdated.Email;
        PhoneNumber = customerUpdated.PhoneNumber;
        Address = customerUpdated.Address;
        Company = customerUpdated.Company;
        Title = customerUpdated.Title;
    }
    
    public void Apply(CustomerEvent @event, ILogger logger)
    {
        logger.LogInformation("Applying event {event} to customer {CustomerId}", @event.EventType, @event.Id);
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