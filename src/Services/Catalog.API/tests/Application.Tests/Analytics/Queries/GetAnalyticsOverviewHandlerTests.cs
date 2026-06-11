using MicroCommerce.Catalog.Application.Analytics.Queries;
using MicroCommerce.Catalog.Domain.Analytics;
using MicroCommerce.Catalog.Domain.Orders;
using MicroCommerce.Catalog.Infrastructure;

namespace MicroCommerce.Catalog.Application.Tests.Analytics.Queries;

public class GetAnalyticsOverviewHandlerTests
{
    // DEMO_NOW = 2026-04-08T16:45:00-07:00 (= 2026-04-09T00:45:00Z) per DemoClock.

    private static Order OrderWithLines(
        int number,
        DateTimeOffset placedAt,
        IReadOnlyList<OrderLine> lines,
        string customerEmail = "buyer@example.com",
        string customerName = "Test Buyer") =>
        new(
            number,
            placedAt,
            customerName,
            customerEmail,
            "San Francisco, CA",
            "1 Main St",
            "San Francisco, CA 94109",
            true,
            "summary",
            "USPS Priority",
            0m,
            0m,
            4m,
            "Visa",
            "4421",
            null,
            0m,
            null,
            string.Empty,
            lines);

    private static OrderLine Line(string sku, string name, int qty, decimal unitPrice) =>
        new(sku, name, "clay", qty, unitPrice, FulfillmentStatus.Awaiting, null, null);

    [Fact]
    public async Task TopProducts_DerivedFromOrderLines_PersimmonVaseFirst()
    {
        await using var db = DbContextFactory.Create();

        // Persimmon vase has highest revenue across orders; Forest bowl second.
        db.Orders.Add(OrderWithLines(2001, new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            new[] { Line("MC-VS-001", "Persimmon vase", 1, 86.00m), Line("MC-BW-014", "Forest bowl", 1, 64.00m) }));
        db.Orders.Add(OrderWithLines(2002, new DateTimeOffset(2026, 4, 7, 11, 8, 0, TimeSpan.FromHours(-7)),
            new[] { Line("MC-VS-001", "Persimmon vase", 2, 86.00m) }));
        db.Orders.Add(OrderWithLines(2003, new DateTimeOffset(2026, 4, 6, 9, 41, 0, TimeSpan.FromHours(-7)),
            new[] { Line("MC-BW-014", "Forest bowl", 1, 64.00m) }));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAnalyticsOverviewHandler(db);
        var result = await handler.Handle(new GetAnalyticsOverviewQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.TopProducts);
        Assert.Equal("Persimmon vase", result.TopProducts[0].Name);
        Assert.Equal("MC-VS-001", result.TopProducts[0].Sku);
        Assert.Equal(3, result.TopProducts[0].Units);
        Assert.Equal(258.00m, result.TopProducts[0].Revenue);
        Assert.True(result.TopProducts.Count <= 6);
    }

    [Fact]
    public async Task RevenueSeries_IncludesOrdersInLast30Days()
    {
        await using var db = DbContextFactory.Create();

        // Order placed today (within last 30 days of DEMO_NOW).
        db.Orders.Add(OrderWithLines(2010, new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            new[] { Line("MC-VS-001", "Persimmon vase", 1, 86.00m) }));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAnalyticsOverviewHandler(db);
        var result = await handler.Handle(new GetAnalyticsOverviewQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.RevenueSeries);
        Assert.Contains(result.RevenueSeries, r => r.Orders > 0 && r.Revenue > 0m);
        Assert.Equal(86.00m, result.RevenueSeries.Sum(r => r.Revenue));
    }

    [Fact]
    public async Task Sources_ReturnsSyntheticSeededRows()
    {
        await using var db = DbContextFactory.Create();
        db.AnalyticsSources.AddRange(
            new AnalyticsSource(1, "Direct", 42.0m, 2184.00m),
            new AnalyticsSource(2, "Instagram", 28.0m, 1456.00m),
            new AnalyticsSource(3, "Email", 18.0m, 936.00m));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAnalyticsOverviewHandler(db);
        var result = await handler.Handle(new GetAnalyticsOverviewQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Sources.Count);
        Assert.Equal("Direct", result.Sources[0].Source);
        Assert.Equal(42.0m, result.Sources[0].Pct);
        Assert.Equal(2184.00m, result.Sources[0].Revenue);
    }

    [Fact]
    public async Task Funnel_ReturnsSyntheticSeededRows()
    {
        await using var db = DbContextFactory.Create();
        db.AnalyticsFunnelStages.AddRange(
            new AnalyticsFunnelStage(1, "Store visits", 1240, 1.0m),
            new AnalyticsFunnelStage(2, "Product views", 892, 0.719m),
            new AnalyticsFunnelStage(3, "Purchased", 46, 0.037m));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetAnalyticsOverviewHandler(db);
        var result = await handler.Handle(new GetAnalyticsOverviewQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Funnel.Count);
        Assert.Equal("Store visits", result.Funnel[0].Label);
        Assert.Equal(1240, result.Funnel[0].Count);
        Assert.Equal(46, result.Funnel[2].Count);
    }
}
