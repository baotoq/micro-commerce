using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Events;

public sealed record StockLowDomainEvent : DomainEvent
{
    public Guid StockItemId { get; }
    public Guid ProductId { get; }
    public int CurrentQuantity { get; }

    public StockLowDomainEvent(Guid stockItemId, Guid productId, int currentQuantity)
    {
        StockItemId = stockItemId;
        ProductId = productId;
        CurrentQuantity = currentQuantity;
    }
}
