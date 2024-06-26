﻿using AspirePlayground.CustomerService.Repository;
using AspirePlayground.IntegrationEvents.CustomerEvents;
using AspirePlayground.ServiceDefaults.Meters;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspirePlayground.CustomerService.Delegates;

public static class CustomerEventDelegates
{
    public static async Task<Ok> ProcessCustomerEvents([FromServices] CustomerMeter customerMeter,
        [FromServices]ILogger<ICustomerRepository> logger,
        [FromServices]ICustomerRepository customerRepository,
        [FromBody]CustomerEvent @event)
    {
        logger.LogInformation("Processing event {event} for {CustomerId}", @event.EventType, @event.CustomerId);
        await customerRepository.AppendEvent(@event);
        customerMeter.CustomerEventsConsumedCounter.Add(1);
        logger.LogInformation("Event {event} was processed", @event.EventType);
        return TypedResults.Ok();
    }
}