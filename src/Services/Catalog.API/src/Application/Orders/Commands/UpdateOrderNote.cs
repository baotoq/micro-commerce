using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Orders.Commands;

public record UpdateOrderNoteCommand(int Number, string Note) : IRequest<OrderDetailDto?>;

public class UpdateOrderNoteHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<UpdateOrderNoteCommand, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(UpdateOrderNoteCommand request, CancellationToken ct)
    {
        // AsTracking() required: DbContext default is NoTracking — see project_ef_notracking_mutating_handlers.
        var order = await db.Orders
            .AsTracking()
            .FirstOrDefaultAsync(o => o.Number == request.Number, ct);
        if (order is null) return null;

        order.UpdateInternalNote(request.Note);

        await db.SaveChangesAsync(ct);

        var dto = await OrderMapping.ToDetailDtoWithCustomerAsync(db, order, ct);
        await publisher.Publish(new OrderUpdatedEvent(order.Number), ct);
        return dto;
    }
}
