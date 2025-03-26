using MediatR;

namespace MicroCommerce.BuildingBlocks.Common;

public abstract record DomainEvent : INotification
{
    public EventId Id { get; }
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
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
