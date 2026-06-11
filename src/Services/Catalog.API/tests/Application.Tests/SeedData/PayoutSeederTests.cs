using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Domain.Payouts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.SeedData;

public class PayoutSeederTests
{
    [Fact]
    public async Task SeedAsync_OnEmptyDb_SeedsTwoPayoutsAndSixLedgerEntries()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await PayoutSeeder.SeedAsync(db, ct);

        Assert.Equal(2, await db.Payouts.CountAsync(ct));
        Assert.Equal(6, await db.LedgerEntries.CountAsync(ct));
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateRecords()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await PayoutSeeder.SeedAsync(db, ct);
        await PayoutSeeder.SeedAsync(db, ct);

        Assert.Equal(2, await db.Payouts.CountAsync(ct));
        Assert.Equal(6, await db.LedgerEntries.CountAsync(ct));
    }

    [Fact]
    public async Task SeedAsync_SeedsExpectedLedgerEntries()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await PayoutSeeder.SeedAsync(db, ct);

        var entries = await db.LedgerEntries.ToListAsync(ct);

        var sale = entries.Single(e => e.Label == "Sale – Persimmon vase");
        Assert.Equal(86.00m, sale.Amount);
        Assert.Equal(LedgerEntryKind.Sale, sale.Kind);
        Assert.Equal("Order #1042 · Apr 8", sale.Subject);
        Assert.Equal(1042, sale.OrderNumber);

        var fee = entries.Single(e => e.Label == "Fee – Persimmon vase");
        Assert.Equal(-3.44m, fee.Amount);
        Assert.Equal(LedgerEntryKind.Fee, fee.Kind);

        var payout = entries.Single(e => e.Label == "Payout – Week of Mar 25");
        Assert.Equal(-247.20m, payout.Amount);
        Assert.Equal(LedgerEntryKind.Payout, payout.Kind);

        Assert.Contains(entries, e => e.Label == "Sale – Forest bowl, lg." && e.Amount == 64.00m);
        Assert.Contains(entries, e => e.Label == "Fee – Forest bowl, lg." && e.Amount == -2.56m);
        Assert.Contains(entries, e => e.Label == "Sale – Ash budstem" && e.Amount == 66.00m);
    }

    [Fact]
    public async Task SeedAsync_SeedsExpectedPayouts()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await PayoutSeeder.SeedAsync(db, ct);

        var payouts = await db.Payouts.ToListAsync(ct);

        Assert.Contains(payouts, p => p.Period == "Week of Mar 25" && p.Amount == 247.20m && !p.IsPending);
        Assert.Contains(payouts, p => p.Period == "Week of Mar 18" && p.Amount == 198.40m && !p.IsPending);
    }
}
