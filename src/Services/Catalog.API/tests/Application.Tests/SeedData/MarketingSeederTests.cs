using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Domain.Marketing;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.SeedData;

public class MarketingSeederTests
{
    [Fact]
    public async Task SeedAsync_OnEmptyDb_AddsSpringLaunchDraft()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await MarketingSeeder.SeedAsync(db, ct);

        var campaign = await db.Campaigns.AsNoTracking().SingleAsync(ct);
        Assert.Equal("Spring Launch", campaign.Name);
        Assert.Equal(CampaignStatus.Draft, campaign.Status);
        Assert.Equal("past-buyers", campaign.AudienceKey);
        Assert.Equal("product-highlight", campaign.TemplateKey);
        Assert.Equal("MC-VS-001", campaign.FeaturedProductSku);
    }

    [Fact]
    public async Task SeedAsync_DraftSubject_HasLength46()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await MarketingSeeder.SeedAsync(db, ct);

        var campaign = await db.Campaigns.AsNoTracking().SingleAsync(ct);
        Assert.Equal(46, campaign.Subject.Length);
    }

    [Fact]
    public async Task SeedAsync_SeedsFollowup_ViewedNotPurchased()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await MarketingSeeder.SeedAsync(db, ct);

        var campaign = await db.Campaigns.AsNoTracking().SingleAsync(ct);
        Assert.True(campaign.FollowupEnabled);
        Assert.NotNull(campaign.Followup);
        Assert.Equal(FollowupKind.ViewedNotPurchased, campaign.Followup!.Kind);
        Assert.Equal(3, campaign.Followup.DelayDays);
    }

    [Fact]
    public async Task SeedAsync_SeedsFixedCreatedAtLiteral()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await MarketingSeeder.SeedAsync(db, ct);

        var campaign = await db.Campaigns.AsNoTracking().SingleAsync(ct);
        Assert.Equal(new DateTimeOffset(2026, 4, 7, 10, 0, 0, TimeSpan.FromHours(-7)), campaign.CreatedAt);
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateCampaign()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        await MarketingSeeder.SeedAsync(db, ct);
        await MarketingSeeder.SeedAsync(db, ct);

        Assert.Equal(1, await db.Campaigns.CountAsync(ct));
    }
}
