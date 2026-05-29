using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class PromotionSeeder
{
    // Idempotent: inserts any seed promo whose code is not already in the DB.
    // The five codes match the legacy /seller/promos fixture so TC-S22 row assertions
    // keep finding their targets. BLOOM seed copy intentionally drifts from the
    // original fixture (no free-shipping kind in MVP) — see design spec §3.3.
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var existing = (await db.Promotions.ToListAsync(ct))
            .Select(p => p.Code.Value)
            .ToHashSet();

        var seeds = new List<(string code, string description, DiscountKind kind, int? percent, decimal? fixedAmount, decimal? minOrder, DateTimeOffset? startsAt, DateTimeOffset? endsAt, bool activate, bool end)>
        {
            ("SPRING20", "sitewide", DiscountKind.Percentage, 20, null, null, new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero), activate: true, end: false),
            ("WELCOME10", "first order", DiscountKind.FixedAmount, null, 10m, 40m, null, null, activate: true, end: false),
            ("STUDIO15", "followers only", DiscountKind.Percentage, 15, null, null, new DateTimeOffset(2026, 4, 22, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 5, 6, 0, 0, 0, TimeSpan.Zero), activate: true, end: false),
            ("BLOOM", "sitewide", DiscountKind.Percentage, 10, null, null, new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 4, 12, 0, 0, 0, TimeSpan.Zero), activate: true, end: true),
            ("FRIENDS", "sitewide", DiscountKind.Percentage, 15, null, null, null, null, activate: false, end: false),
        };

        var added = false;
        foreach (var s in seeds)
        {
            if (existing.Contains(s.code)) continue;

            var promo = new Promotion(
                PromotionCode.From(s.code),
                s.description,
                s.kind,
                s.percent,
                s.fixedAmount,
                s.minOrder,
                s.startsAt,
                s.endsAt);

            if (s.activate) promo.Activate();
            if (s.end) promo.End();

            db.Promotions.Add(promo);
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }
}
