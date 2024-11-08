namespace MicroCommerce.ApiService.Domain.Events;

public abstract record DomainEventBase : IDomainEvent
{
    public string EventType => GetType().FullName!;
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}
