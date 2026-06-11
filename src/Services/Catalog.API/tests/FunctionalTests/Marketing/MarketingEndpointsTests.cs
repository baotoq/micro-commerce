using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Marketing.Dtos;
using MicroCommerce.Catalog.Application.Products.Commands;

namespace MicroCommerce.Catalog.FunctionalTests.Marketing;

public class MarketingEndpointsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly SpyOutputCacheStore _cache = (SpyOutputCacheStore)factory.Services.GetService(typeof(SpyOutputCacheStore))!;

    // The CreateCampaignCommand carries an IClock field that the endpoint rebinds from DI, so
    // the request body omits it (an interface member is not JSON-deserializable). Posting a
    // plain anonymous object leaves Clock null until the endpoint substitutes the demo clock.
    private const string DraftSubject = "New arrivals for spring, handmade just for you";

    private static object NewCampaignBody(string name, string featuredProductSku = "MC-VS-001") => new
    {
        name,
        subject = DraftSubject,
        previewText = "Fresh from the studio.",
        audienceKey = "past-buyers",
        templateKey = "product-highlight",
        featuredProductSku,
        followupEnabled = true,
        followup = new
        {
            enabled = true,
            kind = "ViewedNotPurchased",
            delayDays = 3,
            subject = "Still thinking about it?",
            preview = "Your spring picks are still here.",
        },
    };

    private static string NewName() => $"FN Campaign {Guid.NewGuid():N}"[..24];

    private sealed record PagedCampaigns(IReadOnlyList<CampaignDto> Items, int Total, int Page, int PageSize);

    [Fact]
    public async Task GetMarketingDraft_AfterCreate_ReturnsMappedDraftWithProductLabel()
    {
        var ct = TestContext.Current.CancellationToken;

        // Self-contained: the functional factory does not run seeders, so create the featured
        // product (for the inventory-label join) and a draft campaign referencing it.
        var sku = $"FN-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var productResp = await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Persimmon vase", "Vessels", 86m, 24, "active", PhotoUrls: ["https://example.com/p.jpg"]), ct);
        Assert.Equal(HttpStatusCode.Created, productResp.StatusCode);

        var createResp = await _client.PostAsJsonAsync("/api/marketing/campaigns", NewCampaignBody(NewName(), sku), ct);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.NotNull(created);

        // Plan OQ5: the marketing page resolves drafts via the status filter — verify ours appears.
        var listResp = await _client.GetAsync("/api/marketing/campaigns?status=draft&limit=50", ct);
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var paged = await listResp.Content.ReadFromJsonAsync<PagedCampaigns>(ct);
        Assert.NotNull(paged);
        Assert.Contains(paged!.Items, c => c.Id == created!.Id);

        var response = await _client.GetAsync($"/api/marketing/campaigns/{created!.Id}", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var draft = await response.Content.ReadFromJsonAsync<MarketingDraftDto>(ct);
        Assert.NotNull(draft);
        Assert.Equal(created.Id, draft!.Id);
        Assert.Equal(DraftSubject.Length, draft.SubjectCharCount);
        Assert.Equal("past-buyers", draft.AudienceKey);
        Assert.Equal("product-highlight", draft.TemplateKey);
        Assert.Equal(sku, draft.ProductSku);
        Assert.Equal("24 in stock · $86", draft.ProductInventoryLabel);
        Assert.NotEmpty(draft.AudienceOptions);
        Assert.NotEmpty(draft.TemplateOptions);
    }

    [Fact]
    public async Task GetMarketingDraft_UnknownId_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync($"/api/marketing/campaigns/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCampaign_Valid_Returns201()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync("/api/marketing/campaigns", NewCampaignBody(NewName()), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.NotNull(created);
        Assert.Equal("draft", created!.Status);
        Assert.Equal("MC-VS-001", created.FeaturedProductSku);
    }

    [Fact]
    public async Task CreateCampaign_Successful_EvictsMarketingOutputCacheTag()
    {
        var ct = TestContext.Current.CancellationToken;
        var beforeCount = _cache.EvictedTags.Count(t => t == "marketing");

        await _client.PostAsJsonAsync("/api/marketing/campaigns", NewCampaignBody(NewName()), ct);

        Assert.True(_cache.EvictedTags.Count(t => t == "marketing") > beforeCount,
            "Expected EvictByTagAsync(\"marketing\", ...) after a successful create.");
    }

    [Fact]
    public async Task ScheduleCampaign_Valid_Returns200_AndEvictsTag()
    {
        var ct = TestContext.Current.CancellationToken;

        var create = await _client.PostAsJsonAsync("/api/marketing/campaigns", NewCampaignBody(NewName()), ct);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.NotNull(created);

        var beforeCount = _cache.EvictedTags.Count(t => t == "marketing");
        // Schedule for an instant after the demo-clock CreatedAt so the transition is valid.
        var at = new DateTimeOffset(2026, 5, 1, 9, 0, 0, TimeSpan.FromHours(-7));
        var response = await _client.PostAsJsonAsync(
            $"/api/marketing/campaigns/{created!.Id}/schedule",
            new { id = created.Id, at },
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var scheduled = await response.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.NotNull(scheduled);
        Assert.Equal("scheduled", scheduled!.Status);
        Assert.True(_cache.EvictedTags.Count(t => t == "marketing") > beforeCount,
            "Expected EvictByTagAsync(\"marketing\", ...) after a successful schedule.");
    }

    [Fact]
    public async Task ScheduleCampaign_UnknownId_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();
        var at = new DateTimeOffset(2026, 5, 1, 9, 0, 0, TimeSpan.FromHours(-7));

        var response = await _client.PostAsJsonAsync(
            $"/api/marketing/campaigns/{id}/schedule",
            new { id, at },
            ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendCampaign_FromDraft_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        var create = await _client.PostAsJsonAsync("/api/marketing/campaigns", NewCampaignBody(NewName()), ct);
        var created = await create.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.NotNull(created);

        var response = await _client.PostAsync($"/api/marketing/campaigns/{created!.Id}/send", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sent = await response.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.Equal("sent", sent!.Status);
    }

    [Fact]
    public async Task CancelCampaign_FromDraft_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        var create = await _client.PostAsJsonAsync("/api/marketing/campaigns", NewCampaignBody(NewName()), ct);
        var created = await create.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.NotNull(created);

        var response = await _client.PostAsync($"/api/marketing/campaigns/{created!.Id}/cancel", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cancelled = await response.Content.ReadFromJsonAsync<CampaignDto>(ct);
        Assert.Equal("cancelled", cancelled!.Status);
    }
}
