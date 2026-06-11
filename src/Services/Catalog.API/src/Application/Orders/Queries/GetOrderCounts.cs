using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Orders.Queries;

public record GetOrderCountsQuery : IRequest<OrderCountsDto>;

public class GetOrderCountsHandler(AppDbContext db) : IRequestHandler<GetOrderCountsQuery, OrderCountsDto>
{
    public async Task<OrderCountsDto> Handle(GetOrderCountsQuery request, CancellationToken ct)
    {
        // Status counts in a single SQL pass (stats-query pattern, mirrors GetPromotionStats).
        var counts = await db.Orders.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                All = g.Count(),
                New = g.Count(o => o.Status == OrderStatus.New),
                Packed = g.Count(o => o.Status == OrderStatus.Packed),
                Shipped = g.Count(o => o.Status == OrderStatus.Shipped),
                Delivered = g.Count(o => o.Status == OrderStatus.Delivered),
                RefundRequested = g.Count(o => o.Status == OrderStatus.RefundRequested),
                Cancelled = g.Count(o => o.Status == OrderStatus.Cancelled),
            })
            .FirstOrDefaultAsync(ct);

        if (counts is null)
            return new OrderCountsDto(0, 0, 0, 0, 0, 0, 0, 0m);

        // LifetimeRevenue = sum of Paid (Subtotal + Shipping + Tax) for non-Cancelled orders.
        // Paid/Subtotal are computed from owned Lines, so materialize the non-Cancelled set
        // (demo scale ~46 rows) and sum in memory. TODO: optimize when order volume exceeds 10k.
        var nonCancelled = await db.Orders.AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled)
            .ToListAsync(ct);
        var lifetimeRevenue = nonCancelled.Sum(o => o.Paid);

        return new OrderCountsDto(
            All: counts.All,
            NeedsAction: counts.New + counts.RefundRequested,
            New: counts.New,
            Packed: counts.Packed,
            Shipped: counts.Shipped,
            Delivered: counts.Delivered,
            RefundOrCancel: counts.RefundRequested + counts.Cancelled,
            LifetimeRevenue: lifetimeRevenue);
    }
}
