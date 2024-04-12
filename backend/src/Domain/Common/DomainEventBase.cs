namespace Domain.Common;

public abstract record DomainEventBase : IDomainEvent
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}