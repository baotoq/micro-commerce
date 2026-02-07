using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.Events;

/// <summary>
/// Domain event raised when an order fails (e.g., stock unavailable, payment declined).
/// Thin event containing only the order ID - consumers query for additional data if needed.
/// </summary>
public sealed record OrderFailedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderFailedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
