using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReleaseReservation;

public sealed class ReleaseReservationCommandHandler
    : IRequestHandler<ReleaseReservationCommand, Unit>
{
    private readonly InventoryDbContext _context;

    public ReleaseReservationCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        ReleaseReservationCommand request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.Id == new StockItemId(request.StockItemId), cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException($"Stock item {request.StockItemId} not found.");
        }

        stockItem.ReleaseReservation(new ReservationId(request.ReservationId));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
