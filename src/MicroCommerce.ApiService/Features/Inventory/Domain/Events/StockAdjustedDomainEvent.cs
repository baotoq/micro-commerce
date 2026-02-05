using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Events;

public sealed record StockAdjustedDomainEvent : DomainEvent
{
    public Guid StockItemId { get; }
    public Guid ProductId { get; }
    public int Adjustment { get; }
    public int NewQuantity { get; }

    public StockAdjustedDomainEvent(Guid stockItemId, Guid productId, int adjustment, int newQuantity)
    {
        StockItemId = stockItemId;
        ProductId = productId;
        Adjustment = adjustment;
        NewQuantity = newQuantity;
    }
}
