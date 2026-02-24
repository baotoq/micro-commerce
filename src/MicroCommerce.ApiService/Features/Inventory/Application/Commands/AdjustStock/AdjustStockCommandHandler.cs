using FluentResults;
using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.AdjustStock;

public sealed class AdjustStockCommandHandler
    : IRequestHandler<AdjustStockCommand, Result>
{
    private readonly InventoryDbContext _context;

    public AdjustStockCommandHandler(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        StockItem? stockItem = await _context.StockItems
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
        {
            throw new NotFoundException($"Stock item for product {request.ProductId} not found.");
        }

        try
        {
            stockItem.AdjustStock(request.Adjustment, request.Reason, request.AdjustedBy);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        StockAdjustment adjustment = StockAdjustment.Create(
            stockItem.Id,
            request.Adjustment,
            stockItem.QuantityOnHand,
            request.Reason,
            request.AdjustedBy);

        _context.StockAdjustments.Add(adjustment);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Stock was modified concurrently. Please retry.");
        }

        return Result.Ok();
    }
}
