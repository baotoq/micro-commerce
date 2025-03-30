using MediatR;

namespace MicroCommerce.BuildingBlocks.Common;

public interface IDomainEvent : INotification
{

}

public abstract record DomainEvent : IDomainEvent
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
