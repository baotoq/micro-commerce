using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Domain.Analytics;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.SeedData;

public class AnalyticsSeederTests
{
    [Fact]
    public async Task SeedAsync_OnEmptyDb_SeedsFiveSourcesAndFiveFunnelStages()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await AnalyticsSeeder.SeedAsync(db, ct);

        Assert.Equal(5, await db.AnalyticsSources.CountAsync(ct));
        Assert.Equal(5, await db.AnalyticsFunnelStages.CountAsync(ct));
    }

    [Fact]
    public async Task SeedAsync_SeedsExpectedSourceValues()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await AnalyticsSeeder.SeedAsync(db, ct);

        var sources = await db.AnalyticsSources.OrderBy(s => s.Id).ToListAsync(ct);
        Assert.Equal("Direct", sources[0].Source);
        Assert.Equal(42.0m, sources[0].Pct);
        Assert.Equal(2184.00m, sources[0].Revenue);
        Assert.Equal("Instagram", sources[1].Source);
        Assert.Equal("Email", sources[2].Source);
        Assert.Equal("Search", sources[3].Source);
        Assert.Equal("Other", sources[4].Source);
        Assert.Equal(4.0m, sources[4].Pct);
        Assert.Equal(208.00m, sources[4].Revenue);
    }

    [Fact]
    public async Task SeedAsync_SeedsExpectedFunnelValues()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await AnalyticsSeeder.SeedAsync(db, ct);

        var funnel = await db.AnalyticsFunnelStages.OrderBy(f => f.Id).ToListAsync(ct);
        Assert.Equal("Store visits", funnel[0].Label);
        Assert.Equal(1240, funnel[0].Count);
        Assert.Equal(1.0m, funnel[0].Rate);
        Assert.Equal("Product views", funnel[1].Label);
        Assert.Equal(892, funnel[1].Count);
        Assert.Equal(0.719m, funnel[1].Rate);
        Assert.Equal("Add to cart", funnel[2].Label);
        Assert.Equal("Checkout started", funnel[3].Label);
        Assert.Equal("Purchased", funnel[4].Label);
        Assert.Equal(46, funnel[4].Count);
        Assert.Equal(0.037m, funnel[4].Rate);
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateRows()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await AnalyticsSeeder.SeedAsync(db, ct);
        await AnalyticsSeeder.SeedAsync(db, ct);

        Assert.Equal(5, await db.AnalyticsSources.CountAsync(ct));
        Assert.Equal(5, await db.AnalyticsFunnelStages.CountAsync(ct));
    }

    [Fact]
    public async Task SeedAsync_WithSourcesAlreadyPresent_LeavesThemUntouched()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        db.AnalyticsSources.Add(new AnalyticsSource(1, "Existing", 99.0m, 1.00m));
        await db.SaveChangesAsync(ct);

        await AnalyticsSeeder.SeedAsync(db, ct);

        var sources = await db.AnalyticsSources.ToListAsync(ct);
        Assert.Single(sources);
        Assert.Equal("Existing", sources[0].Source);
        // Funnel was empty, so it still gets seeded.
        Assert.Equal(5, await db.AnalyticsFunnelStages.CountAsync(ct));
    }
}
