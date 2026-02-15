namespace MicroCommerce.BuildingBlocks.Common.Events;

public abstract record DomainEvent : IDomainEvent
{
    public EventId Id { get; }
    public DateTimeOffset DateOccurred { get; init; } = DateTimeOffset.UtcNow;

    protected DomainEvent()
    {
        Id = EventId.New();
    }
}
