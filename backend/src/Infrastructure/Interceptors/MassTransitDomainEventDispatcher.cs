using Domain.Common;
using MassTransit;

namespace Infrastructure.Interceptors;

public class MassTransitDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitDomainEventDispatcher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task DispatchAsync<T>(IEnumerable<T> domainEvents) where T : IDomainEvent
    {
        return Task.WhenAll(domainEvents.Select(e => _publishEndpoint.Publish(e)));
    }
    
    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent
    {
        return _publishEndpoint.Publish(domainEvent);
    }
}