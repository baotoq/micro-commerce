using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReserveStock;

public sealed class ReserveStockCommandHandler
    : IRequestHandler<ReserveStockCommand, Guid>
{
    private readonly InventoryDbContext _context;

    public ReserveStockCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        ReserveStockCommand request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .Include(s => s.Reservations.Where(r => r.ExpiresAt > DateTimeOffset.UtcNow))
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException($"Stock item for product {request.ProductId} not found.");
        }

        var reservationId = stockItem.Reserve(request.Quantity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Stock was modified concurrently. Please retry.");
        }

        return reservationId.Value;
    }
}
