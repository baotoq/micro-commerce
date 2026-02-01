using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Events;

public sealed record StockReservedDomainEvent : DomainEvent
{
    public Guid StockItemId { get; }
    public Guid ProductId { get; }
    public Guid ReservationId { get; }
    public int Quantity { get; }

    public StockReservedDomainEvent(Guid stockItemId, Guid productId, Guid reservationId, int quantity)
    {
        StockItemId = stockItemId;
        ProductId = productId;
        ReservationId = reservationId;
        Quantity = quantity;
    }
}
