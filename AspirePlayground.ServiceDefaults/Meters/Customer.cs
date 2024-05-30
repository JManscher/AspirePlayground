using System.Diagnostics.Metrics;

namespace AspirePlayground.ServiceDefaults.Meters;

public class CustomerMeter(IMeterFactory meterFactory)
{
    private readonly Meter _meter = meterFactory.Create(nameof(CustomerMeter));

    private Counter<int>? _customerEventsConsumedCounter;
    private Counter<int>? _customerChangedEventCounter;

    public Counter<int> CustomerEventsConsumedCounter
    {
        get
        {
            return _customerEventsConsumedCounter ??= _meter.CreateCounter<int>(nameof(CustomerEventsConsumedCounter));
        }
    }

    public Counter<int> CustomerChangedEventCounter
    {
        get
        {
            return _customerChangedEventCounter ??= _meter.CreateCounter<int>(nameof(_customerChangedEventCounter));
        }
    }
}