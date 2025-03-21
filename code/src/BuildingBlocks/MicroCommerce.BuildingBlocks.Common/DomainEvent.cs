namespace MicroCommerce.BuildingBlocks.Common;

public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public bool IsPublished { get; private set; } = false;

    protected DomainEvent() { }

    public void MarkAsPublished()
    {
        IsPublished = true;
    }
}
