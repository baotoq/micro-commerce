using System.ComponentModel.DataAnnotations.Schema;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.BuildingBlocks.Common;

public abstract class BaseAggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected BaseAggregateRoot() : base()
    {
    }

    protected BaseAggregateRoot(TId id) : base(id)
    {
    }

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}
