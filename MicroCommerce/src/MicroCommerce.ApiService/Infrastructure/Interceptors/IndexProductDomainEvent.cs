using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Infrastructure.Interceptors;

public record IndexProductDomainEvent : DomainEventBase
{
    public required string ProductId { get; init; } = "";
}