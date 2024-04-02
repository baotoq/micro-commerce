namespace Domain;

public interface IDomainEvent
{
}

public abstract record DomainEventBase : IDomainEvent
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}