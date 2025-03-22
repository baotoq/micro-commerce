namespace MicroCommerce.BuildingBlocks.Common;

public abstract record DomainEvent
{
    public EventId Id { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public bool IsPublished { get; private set; } = false;

    protected DomainEvent()
    {
        Id = EventId.New();
    }

    public void MarkAsPublished()
    {
        IsPublished = true;
    }
}
