using MediatR;
using MicroCommerce.Catalog.Application.Analytics.Dtos;
using MicroCommerce.Catalog.Application.Orders;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Analytics.Queries;

public record GetDashboardSummaryQuery(IClock Clock) : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryHandler(AppDbContext db) : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var today = request.Clock.Now.Date;

        var newOrders = await db.Orders
            .AsNoTracking()
            .CountAsync(o => o.Status == OrderStatus.New, ct);

        // Revenue = sum of Net for orders placed today. Net is a computed property, so load
        // the (small) set of today's orders with their lines and sum in memory.
        var todaysOrders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.PlacedAt.Date == today)
            .ToListAsync(ct);
        var revenue = todaysOrders.Sum(o => o.Net);

        var lowStock = await db.Products
            .AsNoTracking()
            .CountAsync(p => p.Inventory <= 2, ct);

        var recent = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .OrderByDescending(o => o.PlacedAt)
            .Take(5)
            .ToListAsync(ct);
        var recentOrders = recent.Select(OrderMapping.ToInboxRowDto).ToList();

        var todayItems = new List<DashboardTodayItemDto>
        {
            new($"Pack {newOrders} orders ready to ship", newOrders),
            new($"{AnalyticsMapping.RecentReviews} new reviews to moderate", AnalyticsMapping.RecentReviews),
            new($"Low stock: {lowStock} items", lowStock),
            new("Unread messages: 0", 0),
        };

        return new DashboardSummaryDto(
            AnalyticsMapping.DateLabel,
            newOrders,
            newOrders,
            revenue,
            AnalyticsMapping.RecentReviews,
            todayItems,
            recentOrders);
    }
}
