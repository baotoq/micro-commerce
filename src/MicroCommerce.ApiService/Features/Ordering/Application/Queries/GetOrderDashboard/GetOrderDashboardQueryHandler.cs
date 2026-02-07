using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderDashboard;

public sealed class GetOrderDashboardQueryHandler(OrderingDbContext context)
    : IRequestHandler<GetOrderDashboardQuery, OrderDashboardDto>
{
    private static readonly OrderStatus[] ExcludedStatuses =
        [OrderStatus.Failed, OrderStatus.Cancelled];

    private static readonly OrderStatus[] PendingStatuses =
        [OrderStatus.Submitted, OrderStatus.StockReserved, OrderStatus.Paid];

    public async Task<OrderDashboardDto> Handle(
        GetOrderDashboardQuery request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset? startDate = ParseTimeRange(request.TimeRange);

        IQueryable<Domain.Entities.Order> baseQuery = context.Orders.AsNoTracking();

        if (startDate.HasValue)
        {
            baseQuery = baseQuery.Where(o => o.CreatedAt >= startDate.Value);
        }

        // Total orders and revenue (excluding Failed/Cancelled)
        IQueryable<Domain.Entities.Order> validOrders = baseQuery
            .Where(o => !ExcludedStatuses.Contains(o.Status));

        int totalOrders = await validOrders.CountAsync(cancellationToken);

        decimal revenue = totalOrders > 0
            ? await validOrders.SumAsync(o => o.Total, cancellationToken)
            : 0m;

        decimal averageOrderValue = totalOrders > 0
            ? Math.Round(revenue / totalOrders, 2)
            : 0m;

        // Pending orders count
        int pendingOrders = await baseQuery
            .Where(o => PendingStatuses.Contains(o.Status))
            .CountAsync(cancellationToken);

        // Orders per day for last 7 days (server-side GROUP BY)
        DateTimeOffset sevenDaysAgo = DateTimeOffset.UtcNow.Date.AddDays(-6);

        List<DailyOrderCount> ordersPerDay = await context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyOrderCount(
                DateOnly.FromDateTime(g.Key),
                g.Count()))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        // Fill in missing days with zero counts
        List<DailyOrderCount> filledDays = [];
        for (int i = 0; i < 7; i++)
        {
            DateOnly date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date.AddDays(-6 + i));
            DailyOrderCount? existing = ordersPerDay.FirstOrDefault(d => d.Date == date);
            filledDays.Add(existing ?? new DailyOrderCount(date, 0));
        }

        return new OrderDashboardDto(
            totalOrders,
            revenue,
            averageOrderValue,
            pendingOrders,
            filledDays);
    }

    private static DateTimeOffset? ParseTimeRange(string timeRange)
    {
        return timeRange.ToLowerInvariant() switch
        {
            "today" => DateTimeOffset.UtcNow.Date,
            "7d" => DateTimeOffset.UtcNow.Date.AddDays(-7),
            "30d" => DateTimeOffset.UtcNow.Date.AddDays(-30),
            "all" => null,
            _ => DateTimeOffset.UtcNow.Date
        };
    }
}
