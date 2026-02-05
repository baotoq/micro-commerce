using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.Events;

/// <summary>
/// Domain event raised when a product's status changes (e.g., Draft to Published).
/// </summary>
public sealed record ProductStatusChangedDomainEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string NewStatus { get; }

    public ProductStatusChangedDomainEvent(ProductId productId, ProductStatus newStatus)
    {
        ProductId = productId.Value;
        NewStatus = newStatus.ToString();
    }
}

