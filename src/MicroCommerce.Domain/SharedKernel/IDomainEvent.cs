namespace MicroCommerce.Domain.SharedKernel;

/// <summary>Something that happened inside an aggregate.</summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
