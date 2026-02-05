namespace MicroCommerce.BuildingBlocks.Common.Events;

public abstract record DomainEvent : IDomainEvent
{
    public EventId Id { get; }
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;

    protected DomainEvent()
    {
        Id = EventId.New();
    }
}
