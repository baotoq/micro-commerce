using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(OrderingDbContext context)
    : IRequestHandler<UpdateOrderStatusCommand>
{
    public async Task Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        OrderId orderId = OrderId.From(request.OrderId);

        Domain.Entities.Order order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        switch (request.NewStatus.ToLowerInvariant())
        {
            case "shipped":
                order.Ship();
                break;
            case "delivered":
                order.Deliver();
                break;
            default:
                throw new InvalidOperationException(
                    $"Status '{request.NewStatus}' is not a valid admin transition. Allowed: Shipped, Delivered.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
