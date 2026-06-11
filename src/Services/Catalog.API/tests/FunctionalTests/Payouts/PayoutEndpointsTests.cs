using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Application.Payouts.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Catalog.FunctionalTests.Payouts;

public class PayoutEndpointsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly CatalogWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    // Payouts is read-only (no write endpoints), so seed the database directly
    // through the app's own service scope — the same AppDbContext the endpoints query.
    private async Task SeedNewOrderAsync(int number)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var lines = new List<OrderLine>
        {
            new("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, null, null),
            new("MC-AB-002", "Ash budstem", "rust", 1, 66.00m, FulfillmentStatus.Awaiting, null, "back in stock Tue"),
        };

        var order = new Order(
            number,
            new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            "Sasha Leblanc",
            "sasha.l@gmail.com",
            "San Francisco, CA",
            "820 Sutter St · #4B",
            "San Francisco, CA 94109",
            true,
            "Persimmon vase, Ash budstem",
            "USPS Priority",
            0m,
            0m,
            4m,
            "Visa",
            "4421",
            "USPS",
            9.84m,
            "1lb 4oz",
            "Held until budstem restocks Tue. Sasha OK with split.",
            lines);

        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetPayoutSummary_ReflectsSeededOrders()
    {
        var ct = TestContext.Current.CancellationToken;
        // One New order: Paid = 152.00, Fee = round(152*4/100) = 6.08, LabelCost = 9.84,
        // Net = 152 - 6.08 - 9.84 = 136.08 (canonical #1042 figures, D7).
        await SeedNewOrderAsync(91042);

        var response = await _client.GetAsync("/api/payouts/summary", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<PayoutSummaryDto>(ct);
        Assert.NotNull(summary);
        // Available sums Net of New orders — must include the seeded order's 136.08.
        Assert.True(summary!.Available >= 136.08m,
            $"Expected Available to reflect the seeded New order (>= 136.08), got {summary.Available}.");
        Assert.True(summary.Lifetime >= 136.08m);
        Assert.True(summary.LifetimeOrders >= 1);
    }

    [Fact]
    public async Task GetLedger_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await PayoutSeeder.SeedAsync(db, ct);
        }

        var response = await _client.GetAsync("/api/payouts/ledger?page=1&limit=100", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<LedgerEntryDto>>(ct);
        Assert.NotNull(paged);
        Assert.True(paged!.Total >= 6);
        Assert.Contains(paged.Items, e => e.Label == "Sale – Persimmon vase" && e.Kind == "sale");
    }

    [Fact]
    public async Task GetPayouts_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await PayoutSeeder.SeedAsync(db, ct);
        }

        var response = await _client.GetAsync("/api/payouts?page=1&limit=100", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<PayoutDto>>(ct);
        Assert.NotNull(paged);
        Assert.True(paged!.Total >= 2);
    }
}
