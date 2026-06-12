using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Promotions.Commands;
using MicroCommerce.Catalog.Application.Promotions.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Promotions;

public class PromotionEndpointsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly HttpClient _seller = factory.CreateSellerClient();
    private readonly SpyOutputCacheStore _cache = (SpyOutputCacheStore)factory.Services.GetService(typeof(SpyOutputCacheStore))!;

    private static string NewCode() => $"FN-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    private static CreatePromotionCommand NewPercentageCommand(string code, bool activate = false) =>
        new(code, "sitewide", "percentage", 20, null, null, null, null, activate);

    private static CreatePromotionCommand NewFixedCommand(string code, bool activate = false) =>
        new(code, "first order", "fixed", null, 10m, 40m, null, null, activate);

    [Fact]
    public async Task CreatePromotion_ValidPercentage_Returns201WithLocation()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;

        var response = await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<PromotionDto>(ct);
        Assert.NotNull(created);
        Assert.Equal(code, created!.Code);
        Assert.Equal("percentage", created.Kind);
        Assert.Equal(20, created.PercentValue);
        Assert.Equal("draft", created.Status);
    }

    [Fact]
    public async Task CreatePromotion_ActivateImmediately_PersistsAsActive()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;

        var response = await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code, activate: true), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PromotionDto>(ct);
        Assert.Equal("active", created!.Status);
    }

    [Fact]
    public async Task CreatePromotion_DuplicateCode_Returns409()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        var response = await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePromotion_KnownCode_Returns200WithUpdatedData()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        var update = new UpdatePromotionCommand(code, "followers only", "percentage", 30, null, null, null, null);
        var response = await _seller.PutAsJsonAsync($"/api/promotions/{code}", update, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<PromotionDto>(ct);
        Assert.Equal("followers only", updated!.Description);
        Assert.Equal(30, updated.PercentValue);
    }

    [Fact]
    public async Task UpdatePromotion_UnknownCode_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _seller.PutAsJsonAsync("/api/promotions/NOSUCH-999",
            new UpdatePromotionCommand("NOSUCH-999", "x", "percentage", 5, null, null, null, null), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ActivatePromotion_DraftCode_Returns200ActiveStatus()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        var response = await _seller.PostAsync($"/api/promotions/{code}/activate", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PromotionDto>(ct);
        Assert.Equal("active", dto!.Status);
    }

    [Fact]
    public async Task ActivatePromotion_UnknownCode_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _seller.PostAsync("/api/promotions/NOSUCH-AAA/activate", null, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ActivatePromotion_AlreadyActive_Returns409()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code, activate: true), ct);

        var response = await _seller.PostAsync($"/api/promotions/{code}/activate", null, ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task EndPromotion_ActiveCode_Returns200EndedStatus()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code, activate: true), ct);

        var response = await _seller.PostAsync($"/api/promotions/{code}/end", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PromotionDto>(ct);
        Assert.Equal("ended", dto!.Status);
    }

    [Fact]
    public async Task EndPromotion_StillDraft_Returns409()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        var response = await _seller.PostAsync($"/api/promotions/{code}/end", null, ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeletePromotion_KnownCode_Returns204AndRemoves()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewFixedCommand(code), ct);

        var deleteResponse = await _seller.DeleteAsync($"/api/promotions/{code}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/promotions/{code}", ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePromotion_UnknownCode_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _seller.DeleteAsync("/api/promotions/NOSUCH-BBB", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPromotions_AfterCreates_ReturnsPagedList()
    {
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(NewCode()), ct);
        await _seller.PostAsJsonAsync("/api/promotions", NewFixedCommand(NewCode()), ct);

        var response = await _client.GetAsync("/api/promotions?page=1&limit=100", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedPromotions>(ct);
        Assert.NotNull(paged);
        Assert.True(paged!.Total >= 2);
    }

    [Fact]
    public async Task GetPromotionStats_Always_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync("/api/promotions/stats", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<PromotionStatsDto>(ct);
        Assert.NotNull(stats);
    }

    [Fact]
    public async Task PromotionExists_KnownCode_Returns204()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        var response = await _client.GetAsync($"/api/promotions/{code}/exists", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PromotionExists_UnknownCode_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync("/api/promotions/NOSUCH-CCC/exists", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePromotion_Successful_EvictsPromotionsOutputCacheTag()
    {
        var beforeCount = _cache.EvictedTags.Count(t => t == "promotions");
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;

        await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);

        Assert.True(_cache.EvictedTags.Count(t => t == "promotions") > beforeCount,
            "Expected EvictByTagAsync(\"promotions\", ...) after a successful create.");
    }

    [Fact]
    public async Task RoundTrip_CreateActivateEndDelete_Works()
    {
        var code = NewCode();
        var ct = TestContext.Current.CancellationToken;

        var create = await _seller.PostAsJsonAsync("/api/promotions", NewPercentageCommand(code), ct);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var activate = await _seller.PostAsync($"/api/promotions/{code}/activate", null, ct);
        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);

        var end = await _seller.PostAsync($"/api/promotions/{code}/end", null, ct);
        Assert.Equal(HttpStatusCode.OK, end.StatusCode);

        var delete = await _seller.DeleteAsync($"/api/promotions/{code}", ct);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/promotions/{code}", ct);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    private sealed record PagedPromotions(IReadOnlyList<PromotionDto> Items, int Total, int Page, int PageSize);
}
