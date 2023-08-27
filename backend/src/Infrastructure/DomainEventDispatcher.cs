using Domain;
using MassTransit;

namespace Infrastructure;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
}

public class MassTransitDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitDomainEventDispatcher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        await Task.WhenAll(domainEvents.Select(e => _publishEndpoint.Publish(e)));
    }
}