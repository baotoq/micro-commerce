using MediatR;
using MicroCommerce.Catalog.Application.Analytics.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Analytics.Queries;

public record GetAnalyticsOverviewQuery(IClock Clock) : IRequest<AnalyticsOverviewDto>;

public class GetAnalyticsOverviewHandler(AppDbContext db) : IRequestHandler<GetAnalyticsOverviewQuery, AnalyticsOverviewDto>
{
    public async Task<AnalyticsOverviewDto> Handle(GetAnalyticsOverviewQuery request, CancellationToken ct)
    {
        var now = request.Clock.Now;
        var windowStart = now.AddDays(-30);
        var priorWindowStart = now.AddDays(-60);

        // Load all orders with their owned lines once; project the rest in memory.
        // SelectMany over an owned collection can fail to translate on some providers (see plan R6);
        // for the demo dataset (~46 orders) an in-memory projection is acceptable.
        // TODO: optimize when order volume exceeds 10k.
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .ToListAsync(ct);

        var kpiCards = BuildKpiCards(orders, windowStart, priorWindowStart, now);
        var revenueSeries = BuildRevenueSeries(orders, windowStart, now);
        var topProducts = BuildTopProducts(orders);

        // Sources + Funnel are SYNTHETIC seeded rows, not derived from order data.
        var sources = await db.AnalyticsSources
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SourceBreakdownDto(s.Source, s.Pct, s.Revenue))
            .ToListAsync(ct);

        var funnel = await db.AnalyticsFunnelStages
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .Select(f => new FunnelStageDto(f.Label, f.Count, f.Rate))
            .ToListAsync(ct);

        return new AnalyticsOverviewDto(kpiCards, revenueSeries, sources, topProducts, funnel);
    }

    private static List<KpiPointDto> BuildKpiCards(
        IReadOnlyList<Order> orders,
        DateTimeOffset windowStart,
        DateTimeOffset priorWindowStart,
        DateTimeOffset now)
    {
        static bool IsRealized(Order o) => o.Status is OrderStatus.Shipped or OrderStatus.Delivered;

        var current = orders.Where(o => o.PlacedAt >= windowStart && o.PlacedAt <= now).ToList();
        var prior = orders.Where(o => o.PlacedAt >= priorWindowStart && o.PlacedAt < windowStart).ToList();

        var currentRealized = current.Where(IsRealized).ToList();
        var priorRealized = prior.Where(IsRealized).ToList();

        var currentRevenue = currentRealized.Sum(o => o.Net);
        var priorRevenue = priorRealized.Sum(o => o.Net);

        var currentOrders = currentRealized.Count;
        var priorOrders = priorRealized.Count;

        var currentAov = currentOrders == 0 ? 0m : Math.Round(currentRevenue / currentOrders, 2, MidpointRounding.AwayFromZero);
        var priorAov = priorOrders == 0 ? 0m : Math.Round(priorRevenue / priorOrders, 2, MidpointRounding.AwayFromZero);

        var currentNewCustomers = current.Select(o => o.CustomerEmail).Distinct().Count();
        var priorNewCustomers = prior.Select(o => o.CustomerEmail).Distinct().Count();

        return
        [
            Kpi("Last 30 days", currentRevenue, priorRevenue),
            Kpi("Orders", currentOrders, priorOrders),
            Kpi("Avg. order value", currentAov, priorAov),
            Kpi("New customers", currentNewCustomers, priorNewCustomers),
        ];
    }

    private static KpiPointDto Kpi(string label, decimal value, decimal prior)
    {
        var change = prior == 0m
            ? (value == 0m ? 0m : 100m)
            : Math.Round((value - prior) / prior * 100m, 1, MidpointRounding.AwayFromZero);
        return new KpiPointDto(label, value, change, change >= 0m);
    }

    private static List<RevenuePointDto> BuildRevenueSeries(
        IReadOnlyList<Order> orders,
        DateTimeOffset windowStart,
        DateTimeOffset now)
    {
        return orders
            .Where(o => o.PlacedAt >= windowStart && o.PlacedAt <= now)
            .GroupBy(o => o.PlacedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new RevenuePointDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Sum(o => o.Paid),
                g.Count()))
            .ToList();
    }

    private static List<TopProductDto> BuildTopProducts(IReadOnlyList<Order> orders)
    {
        return orders
            .SelectMany(o => o.Lines)
            .GroupBy(l => new { l.Sku, l.ProductName })
            .Select(g => new TopProductDto(
                g.Key.Sku,
                g.Key.ProductName,
                g.Sum(l => l.Qty),
                g.Sum(l => l.Qty * l.UnitPrice)))
            .OrderByDescending(p => p.Revenue)
            .Take(6)
            .ToList();
    }
}
