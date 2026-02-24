using FluentResults;
using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(OrderingDbContext context)
    : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(
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
                try
                {
                    order.Ship();
                }
                catch (InvalidOperationException ex)
                {
                    return Result.Fail(ex.Message);
                }
                break;
            case "delivered":
                try
                {
                    order.Deliver();
                }
                catch (InvalidOperationException ex)
                {
                    return Result.Fail(ex.Message);
                }
                break;
            default:
                return Result.Fail(
                    $"Status '{request.NewStatus}' is not a valid admin transition. Allowed: Shipped, Delivered.");
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
