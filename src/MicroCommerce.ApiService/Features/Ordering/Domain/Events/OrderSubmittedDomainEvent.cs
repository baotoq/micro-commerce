using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.Events;

/// <summary>
/// Domain event raised when a new order is submitted.
/// Thin event containing only the order ID - consumers query for additional data if needed.
/// </summary>
public sealed record OrderSubmittedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderSubmittedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
