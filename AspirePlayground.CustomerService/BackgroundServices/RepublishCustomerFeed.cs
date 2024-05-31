using System.Threading.Channels;
using AspirePlayground.CustomerService.Model;
using AspirePlayground.CustomerService.Repository;
using Dapr.Client;
public class RepublishCustomerFeed : BackgroundService
{
    private readonly Channel<RepublishCustomerEvents> _queue;
    private readonly ILogger<RepublishCustomerFeed> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly DaprClient _daprClient;

    public RepublishCustomerFeed(IServiceProvider serviceProvider, ILogger<RepublishCustomerFeed> logger, ICustomerRepository customerRepository, DaprClient daprClient)
    {
        _queue = serviceProvider.GetKeyedService<Channel<RepublishCustomerEvents>>("customer-events") ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
        _customerRepository = customerRepository;
        _daprClient = daprClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Republishing customer events");
            var metaData = new Dictionary<string, string>
            {
                { "republished", DateTimeOffset.UtcNow.ToString("O") },
                { "republishId",  item.RequestId.ToString()}
            };
            await foreach (var @event in _customerRepository.ReadAllEvents())
            {
                await _daprClient.PublishEventAsync("pubsub", "customer-changed", @event, metaData);
            }
            _logger.LogInformation("Customer events were republished");

        }
    }
}
