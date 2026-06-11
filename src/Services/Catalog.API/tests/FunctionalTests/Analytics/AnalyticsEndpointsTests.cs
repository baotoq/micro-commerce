using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Analytics.Dtos;
using MicroCommerce.Catalog.Application.Orders.Commands;

namespace MicroCommerce.Catalog.FunctionalTests.Analytics;

public class AnalyticsEndpointsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly SpyOutputCacheStore _cache = (SpyOutputCacheStore)factory.Services.GetService(typeof(SpyOutputCacheStore))!;

    // Unique order numbers per test keep the shared Testcontainers DB collision-free
    // (the unique index on Number is the only conflict surface across tests in this class).
    private static int _next = 9000;
    private static int NextNumber() => Interlocked.Increment(ref _next);

    private static CreateOrderCommand NewOrderCommand(int number, string sku, string productName, decimal unitPrice) =>
        new(
            number,
            new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            "Sasha Leblanc",
            $"buyer{number}@example.com",
            "San Francisco, CA",
            "820 Sutter St · #4B",
            "San Francisco, CA 94109",
            true,
            productName,
            "USPS Priority",
            0m,
            0m,
            4m,
            "Visa",
            "4421",
            null,
            0m,
            null,
            string.Empty,
            new[]
            {
                new CreateOrderLineInput(sku, productName, "clay", 1, unitPrice, "awaiting", null, null),
            });

    [Fact]
    public async Task GetAnalyticsOverview_Returns200_WithTopProducts()
    {
        var ct = TestContext.Current.CancellationToken;
        var create = await _client.PostAsJsonAsync("/api/orders",
            NewOrderCommand(NextNumber(), "MC-VS-001", "Persimmon vase", 86.00m), ct);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var response = await _client.GetAsync("/api/analytics/overview", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var overview = await response.Content.ReadFromJsonAsync<AnalyticsOverviewDto>(ct);
        Assert.NotNull(overview);
        Assert.NotEmpty(overview!.TopProducts);
        Assert.Contains(overview.TopProducts, p => p.Sku == "MC-VS-001" && p.Name == "Persimmon vase");
    }

    [Fact]
    public async Task GetDashboardSummary_Returns200_DateLabel()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync("/api/analytics/dashboard", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>(ct);
        Assert.NotNull(summary);
        Assert.Equal("Tuesday · April 8", summary!.DateLabel);
    }

    [Fact]
    public async Task GetAnalyticsOverview_AfterOrderCreated_EvictsAnalyticsTag()
    {
        var ct = TestContext.Current.CancellationToken;
        var before = _cache.EvictedTags.Count(t => t == "analytics");

        await _client.PostAsJsonAsync("/api/orders",
            NewOrderCommand(NextNumber(), "MC-VS-001", "Persimmon vase", 86.00m), ct);

        Assert.True(_cache.EvictedTags.Count(t => t == "analytics") > before,
            "Expected EvictByTagAsync(\"analytics\", ...) after a successful order create (D6 cache matrix).");
    }
}
