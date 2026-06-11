using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Payouts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class PayoutSeeder
{
    // Idempotent: seeds 2 past payouts and the 6 mock ledger entries from the
    // /seller/payouts fixture. All DateTimeOffset values are FIXED literals anchored
    // to the demo clock (2026-04-08T16:45:00-07:00) so relative-time labels stay stable.
    // NO Random, NO DateTimeOffset.UtcNow.
    //
    // R11: this seeder is independent of the order graph, but must RUN after OrderSeeder
    // so that GetPayoutSummary's derived aggregates line up with the seeded order rows.
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        await SeedPayoutsAsync(db, ct);
        await SeedLedgerAsync(db, ct);
    }

    private static async Task SeedPayoutsAsync(AppDbContext db, CancellationToken ct)
    {
        // Natural key: SentAt + Amount + Period.
        var existing = (await db.Payouts.ToListAsync(ct))
            .Select(p => (p.SentAt, p.Amount, p.Period))
            .ToHashSet();

        var seeds = new List<(DateTimeOffset sentAt, decimal amount, string period, string destination, bool isPending)>
        {
            (new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-7)), 247.20m, "Week of Mar 25", "Chase ···· 8847 · direct deposit", false),
            (new DateTimeOffset(2026, 3, 25, 0, 0, 0, TimeSpan.FromHours(-7)), 198.40m, "Week of Mar 18", "Chase ···· 8847 · direct deposit", false),
        };

        var added = false;
        foreach (var s in seeds)
        {
            if (existing.Contains((s.sentAt, s.amount, s.period))) continue;

            db.Payouts.Add(new Payout(s.sentAt, s.amount, s.period, s.destination, s.isPending));
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }

    private static async Task SeedLedgerAsync(AppDbContext db, CancellationToken ct)
    {
        // Natural key: OccurredAt + Label + Amount + Kind.
        var existing = (await db.LedgerEntries.ToListAsync(ct))
            .Select(e => (e.OccurredAt, e.Label, e.Amount, e.Kind))
            .ToHashSet();

        var apr1 = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-7));
        var mar25 = new DateTimeOffset(2026, 3, 25, 0, 0, 0, TimeSpan.FromHours(-7));
        var mar18 = new DateTimeOffset(2026, 3, 18, 0, 0, 0, TimeSpan.FromHours(-7));

        var seeds = new List<(DateTimeOffset occurredAt, string label, string? subject, decimal amount, LedgerEntryKind kind, int? orderNumber)>
        {
            (apr1, "Sale – Persimmon vase", "Order #1042 · Apr 8", 86.00m, LedgerEntryKind.Sale, 1042),
            (apr1, "Fee – Persimmon vase", "Order #1042 · Apr 8", -3.44m, LedgerEntryKind.Fee, 1042),
            (apr1, "Payout – Week of Mar 25", "Chase ···· 8847 · direct deposit", -247.20m, LedgerEntryKind.Payout, null),
            (mar25, "Sale – Forest bowl, lg.", "Order #1038 · Mar 25", 64.00m, LedgerEntryKind.Sale, 1038),
            (mar25, "Fee – Forest bowl, lg.", "Order #1038 · Mar 25", -2.56m, LedgerEntryKind.Fee, 1038),
            (mar18, "Sale – Ash budstem", "Order #1035 · Mar 18", 66.00m, LedgerEntryKind.Sale, 1035),
        };

        var added = false;
        foreach (var s in seeds)
        {
            if (existing.Contains((s.occurredAt, s.label, s.amount, s.kind))) continue;

            db.LedgerEntries.Add(new LedgerEntry(s.occurredAt, s.label, s.subject, s.amount, s.kind, s.orderNumber));
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }
}
