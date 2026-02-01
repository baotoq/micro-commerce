using MediatR;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;

public sealed class GetStockByProductIdQueryHandler
    : IRequestHandler<GetStockByProductIdQuery, StockInfoDto?>
{
    private readonly InventoryDbContext _context;

    public GetStockByProductIdQueryHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<StockInfoDto?> Handle(
        GetStockByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .AsNoTracking()
            .Include(s => s.Reservations.Where(r => r.ExpiresAt > DateTimeOffset.UtcNow))
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
            return null;

        var available = stockItem.AvailableQuantity;

        return new StockInfoDto(
            stockItem.ProductId,
            stockItem.QuantityOnHand,
            available,
            IsInStock: available > 0,
            IsLowStock: available > 0 && available <= 10);
    }
}
