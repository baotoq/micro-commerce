namespace MicroCommerce.Domain.SharedKernel;

/// <summary>
/// Base type for aggregates. State changes only through the aggregate's own methods, which raise
/// domain events for whatever must react to them.
/// </summary>
public abstract class AggregateRoot<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; init; } = default!;

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IDomainEvent e) => _domainEvents.Add(e);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
