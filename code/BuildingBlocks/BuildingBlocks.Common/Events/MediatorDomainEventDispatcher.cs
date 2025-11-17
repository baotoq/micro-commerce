using MediatR;

namespace MicroCommerce.BuildingBlocks.Common.Events;

public class MediatorDomainEventDispatcher(IMediator mediator) : IDomainEventDispatcher
{
    public Task DispatchAsync<T>(IEnumerable<T> domainEvents) where T : IDomainEvent
    {
        return Task.WhenAll(domainEvents.Select(e => mediator.Publish(e)));
    }

    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent
    {
        return mediator.Publish(domainEvent);
    }
}
