using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.Events;

/// <summary>
/// Domain event raised when an order payment is confirmed.
/// Thin event containing only the order ID - consumers query for additional data if needed.
/// </summary>
public sealed record OrderPaidDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderPaidDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
