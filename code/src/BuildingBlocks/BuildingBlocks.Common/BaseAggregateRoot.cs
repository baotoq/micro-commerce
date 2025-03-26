using System.ComponentModel.DataAnnotations.Schema;

namespace MicroCommerce.BuildingBlocks.Common;

public abstract class BaseAggregateRoot<TId>(TId id) : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];
    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public TId Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
    public void ClearDomainEvents() => _domainEvents.Clear();
}
