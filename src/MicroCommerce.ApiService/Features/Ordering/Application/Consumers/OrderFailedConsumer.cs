using MassTransit;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Sets order status to Failed with reason.
/// Idempotent: skips if order is already in Failed status.
/// </summary>
public sealed class OrderFailedConsumer(
    OrderingDbContext orderingDb,
    ILogger<OrderFailedConsumer> logger) : IConsumer<OrderFailed>
{
    public async Task Consume(ConsumeContext<OrderFailed> context)
    {
        OrderId orderId = OrderId.From(context.Message.OrderId);

        Domain.Entities.Order? order = await orderingDb.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, context.CancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for failure marking", context.Message.OrderId);
            return;
        }

        // Idempotent: skip if already failed
        if (order.Status == OrderStatus.Failed)
        {
            logger.LogInformation("Order {OrderId} already in Failed status, skipping", context.Message.OrderId);
            return;
        }

        order.MarkAsFailed(context.Message.Reason);
        await orderingDb.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Order {OrderId} marked as failed: {Reason}",
            context.Message.OrderId,
            context.Message.Reason);
    }
}
