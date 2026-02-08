using MediatR;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetAdjustmentHistory;

public sealed class GetAdjustmentHistoryQueryHandler
    : IRequestHandler<GetAdjustmentHistoryQuery, List<AdjustmentDto>>
{
    private readonly InventoryDbContext _context;

    public GetAdjustmentHistoryQueryHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdjustmentDto>> Handle(
        GetAdjustmentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _context.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
            return [];

        var adjustments = await _context.StockAdjustments
            .AsNoTracking()
            .Where(a => a.StockItemId == stockItem.Id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AdjustmentDto(
                a.Id.Value,
                a.Adjustment,
                a.QuantityAfter,
                a.Reason,
                a.AdjustedBy,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return adjustments;
    }
}
