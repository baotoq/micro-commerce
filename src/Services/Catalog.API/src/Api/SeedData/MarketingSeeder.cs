using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Marketing;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class MarketingSeeder
{
    // Idempotent: inserts the seed campaign only when its Name is not already in the DB.
    // Seeds exactly one Draft campaign so the marketing page's ?status=draft&limit=1 fetch
    // (plan OQ5) always resolves to the same campaign across re-seeds.
    //
    // Subject is the canonical 46-char string (= SubjectCharCount 46, plan D2 #6). The plan's
    // em-dash literal renders as 48 UTF-16 units, so we use the true 46-char variant the
    // Application tests assert against.
    private const string Subject46 = "New arrivals for spring, handmade just for you";

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var existing = (await db.Campaigns.ToListAsync(ct))
            .Select(c => c.Name)
            .ToHashSet();

        if (existing.Contains("Spring Launch")) return;

        var followup = new CampaignFollowup(
            enabled: true,
            kind: FollowupKind.ViewedNotPurchased,
            delayDays: 3,
            subject: "Still thinking about it?",
            preview: "Your spring picks are still here.");

        var campaign = new Campaign(
            name: "Spring Launch",
            subject: Subject46,
            previewText: "Fresh from the studio.",
            audienceKey: "past-buyers",
            templateKey: "product-highlight",
            followupEnabled: true,
            followup: followup,
            featuredProductSku: "MC-VS-001",
            createdAt: new DateTimeOffset(2026, 4, 7, 10, 0, 0, TimeSpan.FromHours(-7)));

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(ct);
    }
}
