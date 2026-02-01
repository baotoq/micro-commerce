using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.Events;

public sealed record StockReleasedDomainEvent : DomainEvent
{
    public Guid StockItemId { get; }
    public Guid ProductId { get; }
    public Guid ReservationId { get; }
    public int Quantity { get; }

    public StockReleasedDomainEvent(Guid stockItemId, Guid productId, Guid reservationId, int quantity)
    {
        StockItemId = stockItemId;
        ProductId = productId;
        ReservationId = reservationId;
        Quantity = quantity;
    }
}
