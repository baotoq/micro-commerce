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

        // Orders per day for last 7 days.
        // Use explicit UTC offset to avoid Npgsql rejecting non-UTC DateTimeOffset values.
        // Fetch CreatedAt timestamps and group in memory — EF/Npgsql cannot translate
        // DateTimeOffset.Date + GroupBy + DateOnly.FromDateTime to SQL in this EF version.
        DateTimeOffset sevenDaysAgo = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(-6), TimeSpan.Zero);

        List<DateTimeOffset> recentOrderTimestamps = await context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .Select(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        // Group by date in memory (UTC date)
        Dictionary<DateOnly, int> countsByDay = recentOrderTimestamps
            .GroupBy(ts => DateOnly.FromDateTime(ts.UtcDateTime.Date))
            .ToDictionary(g => g.Key, g => g.Count());

        // Fill in missing days with zero counts
        List<DailyOrderCount> filledDays = [];
        for (int i = 0; i < 7; i++)
        {
            DateOnly date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(-6 + i));
            filledDays.Add(new DailyOrderCount(date, countsByDay.GetValueOrDefault(date, 0)));
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
        // Use explicit UTC offset to avoid Npgsql rejecting non-UTC DateTimeOffset values.
        // DateTimeOffset.UtcNow.Date returns a DateTime with Kind=Local which gets an implicit
        // local timezone offset when converted to DateTimeOffset.
        DateTime utcToday = DateTimeOffset.UtcNow.UtcDateTime.Date;
        return timeRange.ToLowerInvariant() switch
        {
            "today" => new DateTimeOffset(utcToday, TimeSpan.Zero),
            "7d" => new DateTimeOffset(utcToday.AddDays(-7), TimeSpan.Zero),
            "30d" => new DateTimeOffset(utcToday.AddDays(-30), TimeSpan.Zero),
            "all" => null,
            _ => new DateTimeOffset(utcToday, TimeSpan.Zero)
        };
    }
}
