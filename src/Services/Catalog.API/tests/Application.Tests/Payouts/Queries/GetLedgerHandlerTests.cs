using MicroCommerce.Catalog.Application.Payouts.Queries;
using MicroCommerce.Catalog.Domain.Payouts;

namespace MicroCommerce.Catalog.Application.Tests.Payouts.Queries;

public class GetLedgerHandlerTests
{
    private static LedgerEntry Entry(DateTimeOffset occurredAt, string label, decimal amount, LedgerEntryKind kind) =>
        new(occurredAt, label, $"Order · {label}", amount, kind, null);

    [Fact]
    public async Task ReturnsEntriesOrderedByOccurredAtDesc()
    {
        await using var db = DbContextFactory.Create();
        var older = new DateTimeOffset(2026, 3, 18, 0, 0, 0, TimeSpan.FromHours(-7));
        var middle = new DateTimeOffset(2026, 3, 25, 0, 0, 0, TimeSpan.FromHours(-7));
        var newest = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-7));

        // Inserted out of order on purpose.
        db.LedgerEntries.Add(Entry(middle, "Sale – Forest bowl", 64.00m, LedgerEntryKind.Sale));
        db.LedgerEntries.Add(Entry(newest, "Sale – Persimmon vase", 86.00m, LedgerEntryKind.Sale));
        db.LedgerEntries.Add(Entry(older, "Sale – Ash budstem", 66.00m, LedgerEntryKind.Fee));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetLedgerHandler(db);
        var result = await handler.Handle(new GetLedgerQuery(1, 10), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Total);
        Assert.Equal(newest, result.Items[0].OccurredAt);
        Assert.Equal(middle, result.Items[1].OccurredAt);
        Assert.Equal(older, result.Items[2].OccurredAt);
        Assert.Equal("sale", result.Items[0].Kind);
        Assert.Equal("fee", result.Items[2].Kind);
    }

    [Fact]
    public async Task Pagination_ReturnsCorrectPage()
    {
        await using var db = DbContextFactory.Create();
        for (var i = 0; i < 5; i++)
            db.LedgerEntries.Add(Entry(
                new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-7)).AddDays(-i),
                $"Sale #{i}", 10m, LedgerEntryKind.Sale));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetLedgerHandler(db);
        var result = await handler.Handle(new GetLedgerQuery(2, 2), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }
}
