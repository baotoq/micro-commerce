using MicroCommerce.Catalog.Application.Marketing.Queries;
using MicroCommerce.Catalog.Domain.Marketing;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Marketing.Queries;

public class GetMarketingDraftHandlerTests
{
    // 46-char subject: the plan's canonical SubjectCharCount is 46 (D2 #6), but the literal it
    // quotes ("...spring — handmade, just for you") is actually 48 UTF-16 units. We seed a true
    // 46-char subject so the assertion validates the real Subject.Length derivation.
    private const string Subject46 = "New arrivals for spring, handmade just for you";

    private static Campaign SpringLaunch(string? featuredProductSku = null) =>
        new(
            "Spring Launch",
            Subject46,
            "Fresh from the studio.",
            "past-buyers",
            "product-highlight",
            followupEnabled: true,
            followup: new CampaignFollowup(true, FollowupKind.ViewedNotPurchased, 3, "Still thinking about it?", "Your spring picks are still here."),
            featuredProductSku: featuredProductSku,
            createdAt: new DateTimeOffset(2026, 4, 7, 10, 0, 0, TimeSpan.FromHours(-7)));

    [Fact]
    public async Task Handle_UnknownId_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetMarketingDraftHandler(db);

        var result = await handler.Handle(
            new GetMarketingDraftQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_KnownId_ReturnsSubjectCharCount46()
    {
        await using var db = DbContextFactory.Create();
        var campaign = SpringLaunch();
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetMarketingDraftHandler(db);
        var result = await handler.Handle(
            new GetMarketingDraftQuery(campaign.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(Subject46, result!.Subject);
        Assert.Equal(46, result.SubjectCharCount);
        Assert.Equal("past-buyers", result.AudienceKey);
        Assert.Equal("product-highlight", result.TemplateKey);
        Assert.NotEmpty(result.AudienceOptions);
        Assert.NotEmpty(result.TemplateOptions);
    }

    [Fact]
    public async Task Handle_KnownId_WithProductSku_JoinsInventoryLabel()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(
            Sku.From("MC-VS-001"),
            "Persimmon vase",
            "Vases",
            price: 86m,
            inventory: 24,
            ProductStatus.Active,
            photoUrls: ["https://example.com/p.jpg"]));
        var campaign = SpringLaunch(featuredProductSku: "MC-VS-001");
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetMarketingDraftHandler(db);
        var result = await handler.Handle(
            new GetMarketingDraftQuery(campaign.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("MC-VS-001", result!.ProductSku);
        Assert.Equal("Persimmon vase", result.ProductName);
        Assert.Equal("24 in stock · $86", result.ProductInventoryLabel);
    }
}
