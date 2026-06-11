using MicroCommerce.Catalog.Application.Payouts.Queries;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using MicroCommerce.Catalog.Domain.Payouts;

namespace MicroCommerce.Catalog.Application.Tests.Payouts.Queries;

public class GetPayoutSummaryHandlerTests
{
    // Fixed demo clock anchored to 2026-04-08T16:45:00-07:00 (the project demo anchor).
    private sealed class DemoClock : IClock
    {
        public DateTimeOffset Now => new DateTimeOffset(2026, 4, 8, 16, 45, 0, TimeSpan.FromHours(-7));
    }

    private static Order NewOrder(int number, decimal unitPrice, int qty = 1) =>
        new(
            number,
            new DateTimeOffset(2026, 4, 8, 9, 0, 0, TimeSpan.FromHours(-7)),
            "Sasha Leblanc",
            "sasha.l@gmail.com",
            "San Francisco, CA",
            "820 Sutter St",
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
            "",
            new[] { new OrderLine("MC-VS-001", "Persimmon vase", "clay", qty, unitPrice, FulfillmentStatus.Awaiting, null, null) });

    [Fact]
    public async Task Available_SumsNetOfNewOrders()
    {
        await using var db = DbContextFactory.Create();
        // Two New orders: Net = Paid - Fee - LabelCost. Paid = 100; Fee = round(100*4/100)=4; Net = 96 each.
        db.Orders.Add(NewOrder(2001, 100m));
        db.Orders.Add(NewOrder(2002, 100m));
        // A Packed order must NOT count toward Available.
        var packed = NewOrder(2003, 200m);
        packed.Pack();
        db.Orders.Add(packed);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPayoutSummaryHandler(db);
        var result = await handler.Handle(new GetPayoutSummaryQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.Equal(192m, result.Available);
    }

    [Fact]
    public async Task Lifetime_ExcludesCancelled()
    {
        await using var db = DbContextFactory.Create();
        // Net 96 each (Paid 100, Fee 4).
        db.Orders.Add(NewOrder(3001, 100m));
        var cancelled = NewOrder(3002, 100m);
        cancelled.Cancel();
        db.Orders.Add(cancelled);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPayoutSummaryHandler(db);
        var result = await handler.Handle(new GetPayoutSummaryQuery(new DemoClock()), TestContext.Current.CancellationToken);

        Assert.Equal(96m, result.Lifetime);
        Assert.Equal(1, result.LifetimeOrders);
    }

    [Fact]
    public async Task AvailableSendsOn_IsLastPayoutPlus7Days()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(NewOrder(4001, 100m));
        // Two payouts; the most recent SentAt should win.
        db.Payouts.Add(new Payout(
            new DateTimeOffset(2026, 3, 25, 0, 0, 0, TimeSpan.FromHours(-7)),
            247.20m, "Week of Mar 18", "Chase ···· 8847", false));
        db.Payouts.Add(new Payout(
            new DateTimeOffset(2026, 4, 8, 0, 0, 0, TimeSpan.FromHours(-7)),
            136.08m, "Week of Mar 25", "Chase ···· 8847", false));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetPayoutSummaryHandler(db);
        var result = await handler.Handle(new GetPayoutSummaryQuery(new DemoClock()), TestContext.Current.CancellationToken);

        // Last payout SentAt (Apr 8) + 7 days = Apr 15.
        Assert.Equal(new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.FromHours(-7)), result.AvailableSendsOn);
    }

    [Fact]
    public async Task AvailableSendsOn_FallsBackToClockPlus7_WhenNoPayouts()
    {
        await using var db = DbContextFactory.Create();
        db.Orders.Add(NewOrder(5001, 100m));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var clock = new DemoClock();
        var handler = new GetPayoutSummaryHandler(db);
        var result = await handler.Handle(new GetPayoutSummaryQuery(clock), TestContext.Current.CancellationToken);

        Assert.Equal(clock.Now.AddDays(7), result.AvailableSendsOn);
    }
}
