using MediatR;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockLevels;

public sealed class GetStockLevelsQueryHandler
    : IRequestHandler<GetStockLevelsQuery, List<StockInfoDto>>
{
    private readonly InventoryDbContext _context;

    public GetStockLevelsQueryHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockInfoDto>> Handle(
        GetStockLevelsQuery request,
        CancellationToken cancellationToken)
    {
        var stockItems = await _context.StockItems
            .AsNoTracking()
            .Include(s => s.Reservations.Where(r => r.ExpiresAt > DateTimeOffset.UtcNow))
            .Where(s => request.ProductIds.Contains(s.ProductId))
            .ToListAsync(cancellationToken);

        var result = new List<StockInfoDto>(request.ProductIds.Count);

        foreach (var productId in request.ProductIds)
        {
            var stockItem = stockItems.FirstOrDefault(s => s.ProductId == productId);

            if (stockItem is null)
            {
                result.Add(new StockInfoDto(productId, 0, 0, IsInStock: false, IsLowStock: false));
                continue;
            }

            var available = stockItem.AvailableQuantity;
            result.Add(new StockInfoDto(
                stockItem.ProductId,
                stockItem.QuantityOnHand,
                available,
                IsInStock: available > 0,
                IsLowStock: available > 0 && available <= 10));
        }

        return result;
    }
}
