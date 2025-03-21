namespace MicroCommerce.BuildingBlocks.Common;

public abstract class BaseAggregateRoot<TId>(TId id)
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public TId Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
