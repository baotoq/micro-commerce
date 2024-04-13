using Domain.Common;

namespace Infrastructure.Interceptors;

public record IndexProductDomainEvent : DomainEventBase
{
    public required string ProductId { get; init; } = "";
}