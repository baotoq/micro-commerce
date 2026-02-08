using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.Events;

/// <summary>
/// Domain event raised when a new product is created.
/// Thin event containing only the product ID - consumers query for additional data if needed.
/// </summary>
public sealed record ProductCreatedDomainEvent : DomainEvent
{
    public Guid ProductId { get; }

    public ProductCreatedDomainEvent(ProductId productId)
    {
        ProductId = productId.Value;
    }
}

