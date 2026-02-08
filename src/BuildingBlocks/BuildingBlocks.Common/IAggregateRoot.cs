using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.BuildingBlocks.Common;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
