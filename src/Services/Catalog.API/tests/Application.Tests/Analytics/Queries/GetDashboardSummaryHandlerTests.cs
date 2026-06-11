using MicroCommerce.Catalog.Application.Analytics.Queries;
using MicroCommerce.Catalog.Domain.Orders;
using MicroCommerce.Catalog.Infrastructure;

namespace MicroCommerce.Catalog.Application.Tests.Analytics.Queries;

public class GetDashboardSummaryHandlerTests
{
    // DEMO_NOW = 2026-04-08T16:45:00-07:00 (= 2026-04-09T00:45:00Z) per DemoClock.

    private static Order OrderWithLines(
        int number,
        DateTimeOffset placedAt,
        IReadOnlyList<OrderLine> lines) =>
        new(
            number,
            placedAt,
            "Test Buyer",
            "buyer@example.com",
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

    private static Order NewOrder(int number, DateTimeOffset placedAt) =>
        OrderWithLines(number, placedAt, new[] { Line("MC-VS-001", "Persimmon vase", 1, 86.00m) });

    [Fact]
    public async Task OrdersToShip_EqualsCountOfNewOrders()
    {
        await using var db = DbContextFactory.Create();

        // 3 New orders.
        db.Orders.Add(NewOrder(3001, new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7))));
        db.Orders.Add(NewOrder(3002, new DateTimeOffset(2026, 4, 8, 11, 8, 0, TimeSpan.FromHours(-7))));
        db.Orders.Add(NewOrder(3003, new DateTimeOffset(2026, 4, 8, 9, 41, 0, TimeSpan.FromHours(-7))));

        // 1 Packed order (not New) — should not count toward OrdersToShip.
        var packed = NewOrder(3004, new DateTimeOffset(2026, 4, 7, 14, 30, 0, TimeSpan.FromHours(-7)));
        packed.Pack();
        db.Orders.Add(packed);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardSummaryHandler(db);
        var result = await handler.Handle(new GetDashboardSummaryQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.NewOrders);
        Assert.Equal(3, result.OrdersToShip);
    }

    [Fact]
    public async Task Revenue_SumsNetForToday()
    {
        await using var db = DbContextFactory.Create();

        // Two orders placed today (2026-04-08 in the demo offset).
        db.Orders.Add(NewOrder(3010, new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7))));
        db.Orders.Add(NewOrder(3011, new DateTimeOffset(2026, 4, 8, 9, 0, 0, TimeSpan.FromHours(-7))));

        // One order placed yesterday — excluded from today's revenue.
        db.Orders.Add(NewOrder(3012, new DateTimeOffset(2026, 4, 7, 9, 0, 0, TimeSpan.FromHours(-7))));

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardSummaryHandler(db);
        var result = await handler.Handle(new GetDashboardSummaryQuery(new DemoClock()), TestContext.Current.CancellationToken);

        // Each order: Paid = 86.00, Fee = round(86 * 4 / 100) = 3.44, Net = 82.56. Two today => 165.12.
        Assert.Equal(165.12m, result.Revenue);
    }

    [Fact]
    public async Task RecentOrders_Returns5MostRecentByPlacedAt()
    {
        await using var db = DbContextFactory.Create();

        // 7 orders with descending placement; expect the 5 most recent by PlacedAt.
        for (var i = 0; i < 7; i++)
            db.Orders.Add(NewOrder(4000 + i, new DateTimeOffset(2026, 4, 8, 0, 0, 0, TimeSpan.FromHours(-7)).AddDays(-i)));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardSummaryHandler(db);
        var result = await handler.Handle(new GetDashboardSummaryQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.RecentOrders.Count);
        // Most recent first (order 4000 placed at Apr 8, 4001 at Apr 7, ...).
        Assert.Equal(4000, result.RecentOrders[0].Number);
        Assert.Equal(4004, result.RecentOrders[4].Number);
        Assert.True(result.RecentOrders[0].PlacedAt >= result.RecentOrders[4].PlacedAt);
    }
}
