using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Analytics;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class AnalyticsSeeder
{
    // Idempotent: seeds the SYNTHETIC traffic-source breakdown and conversion-funnel rows only
    // when the tables are empty. Unlike orders/payouts, analytics sources and funnel stages are
    // NOT derived from real data (there is no referrer/clickstream domain) — they hold fixed
    // seeded literals. Values are verbatim from plan §2.5 SeedData; chosen plausible against the
    // 46 seeded orders. NO Random, NO DateTimeOffset.UtcNow.
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var added = false;

        if (!await db.AnalyticsSources.AnyAsync(ct))
        {
            db.AnalyticsSources.AddRange(
                new AnalyticsSource(1, "Direct", 42.0m, 2184.00m),
                new AnalyticsSource(2, "Instagram", 28.0m, 1456.00m),
                new AnalyticsSource(3, "Email", 18.0m, 936.00m),
                new AnalyticsSource(4, "Search", 8.0m, 416.00m),
                new AnalyticsSource(5, "Other", 4.0m, 208.00m));
            added = true;
        }

        if (!await db.AnalyticsFunnelStages.AnyAsync(ct))
        {
            db.AnalyticsFunnelStages.AddRange(
                new AnalyticsFunnelStage(1, "Store visits", 1240, 1.0m),
                new AnalyticsFunnelStage(2, "Product views", 892, 0.719m),
                new AnalyticsFunnelStage(3, "Add to cart", 318, 0.257m),
                new AnalyticsFunnelStage(4, "Checkout started", 127, 0.103m),
                new AnalyticsFunnelStage(5, "Purchased", 46, 0.037m));
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }
}
