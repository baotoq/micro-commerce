using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Orders.Commands;

public record CancelOrderCommand(int Number) : IRequest<Result<OrderDetailDto>>;

public class CancelOrderHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<CancelOrderCommand, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        // AsTracking() required: DbContext default is NoTracking — see project_ef_notracking_mutating_handlers.
        var order = await db.Orders
            .AsTracking()
            .FirstOrDefaultAsync(o => o.Number == request.Number, ct);
        if (order is null)
            return Result<OrderDetailDto>.Conflict("ORDER_NOT_FOUND", $"Order #{request.Number} was not found.");

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderDetailDto>.Conflict("ORDER_INVALID_TRANSITION", ex.Message);
        }

        await db.SaveChangesAsync(ct);

        var dto = await OrderMapping.ToDetailDtoWithCustomerAsync(db, order, ct);
        await publisher.Publish(new OrderStatusChangedEvent(order.Number, dto.Status), ct);
        return Result<OrderDetailDto>.Success(dto);
    }
}
