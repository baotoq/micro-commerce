using MediatR;

namespace MicroCommerce.BuildingBlocks.Common.Events;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
    new Task Handle(TDomainEvent @event, CancellationToken cancellationToken);
}
