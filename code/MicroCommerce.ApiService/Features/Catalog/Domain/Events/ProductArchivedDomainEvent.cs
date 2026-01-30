using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.Events;

/// <summary>
/// Domain event raised when a product is archived (soft deleted).
/// </summary>
public sealed record ProductArchivedDomainEvent : DomainEvent
{
    public Guid ProductId { get; }

    public ProductArchivedDomainEvent(ProductId productId)
    {
        ProductId = productId.Value;
    }
}

