using MassTransit;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using MicroCommerce.ApiService.Features.Ordering.Domain.Events;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Bridges the domain event (OrderSubmittedDomainEvent) to the checkout saga (CheckoutStarted).
/// Domain events are thin (ID-only); this consumer loads order data and publishes the saga-initiating event.
/// </summary>
public sealed class OrderSubmittedConsumer(OrderingDbContext context)
    : IConsumer<OrderSubmittedDomainEvent>
{
    public async Task Consume(ConsumeContext<OrderSubmittedDomainEvent> context1)
    {
        OrderId orderId = OrderId.From(context1.Message.OrderId);

        Domain.Entities.Order? order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
        {
            return;
        }

        List<CheckoutItem> items = order.Items
            .Select(i => new CheckoutItem { ProductId = i.ProductId, Quantity = i.Quantity })
            .ToList();

        await context1.Publish(new CheckoutStarted
        {
            OrderId = order.Id.Value,
            BuyerId = order.BuyerId,
            BuyerEmail = order.BuyerEmail,
            Items = items
        });
    }
}
