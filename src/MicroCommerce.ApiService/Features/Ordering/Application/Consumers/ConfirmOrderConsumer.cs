using MassTransit;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Sets order status to Confirmed after successful payment and stock deduction.
/// </summary>
public sealed class ConfirmOrderConsumer(
    OrderingDbContext orderingDb,
    ILogger<ConfirmOrderConsumer> logger) : IConsumer<ConfirmOrder>
{
    public async Task Consume(ConsumeContext<ConfirmOrder> context)
    {
        OrderId orderId = OrderId.From(context.Message.OrderId);

        Domain.Entities.Order? order = await orderingDb.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, context.CancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for confirmation", context.Message.OrderId);
            return;
        }

        order.Confirm();
        await orderingDb.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Order {OrderId} confirmed", context.Message.OrderId);
    }
}
