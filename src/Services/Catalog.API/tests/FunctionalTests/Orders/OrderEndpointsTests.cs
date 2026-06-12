using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Orders;

public class OrderEndpointsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly HttpClient _seller = factory.CreateSellerClient();
    private readonly SpyOutputCacheStore _cache = (SpyOutputCacheStore)factory.Services.GetService(typeof(SpyOutputCacheStore))!;

    // Unique high order numbers per test so a shared (possibly seeded) DB never collides.
    private static int _next = 900_000;
    private static int NextNumber() => Interlocked.Increment(ref _next);

    private static CreateOrderCommand NewOrderCommand(int number) =>
        new(
            Number: number,
            PlacedAt: new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            CustomerName: "Sasha Leblanc",
            CustomerEmail: "sasha.l@gmail.com",
            CityState: "San Francisco, CA",
            ShipLine1: "820 Sutter St · #4B",
            ShipLine2: "San Francisco, CA 94109",
            BillSameAsShip: true,
            ItemsSummary: "Persimmon vase",
            ShippingMethod: "USPS Priority",
            ShippingPaid: 0m,
            Tax: 0m,
            FeePct: 4m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4421",
            LabelCarrier: "USPS",
            LabelCost: 9.84m,
            LabelWeightLabel: "1lb 4oz",
            InternalNote: "",
            Lines:
            [
                new CreateOrderLineInput("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, "awaiting", null, null),
            ]);

    [Fact]
    public async Task CreateOrder_Valid_Returns201WithLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var number = NextNumber();

        var response = await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(number), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<OrderDetailDto>(ct);
        Assert.NotNull(created);
        Assert.Equal(number, created!.Number);
        Assert.Equal("new", created.Status);
    }

    [Fact]
    public async Task CreateOrder_DuplicateNumber_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;
        var number = NextNumber();
        await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(number), ct);

        var response = await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(number), ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_TabNeedsAction_ReturnsFiltered()
    {
        var ct = TestContext.Current.CancellationToken;
        var number = NextNumber();
        await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(number), ct);

        var response = await _client.GetAsync("/api/orders?page=1&limit=200&tab=needs-action", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedOrders>(ct);
        Assert.NotNull(paged);
        // The just-created New order is in the needs-action tab.
        Assert.Contains(paged!.Items, o => o.Number == number);
        // Every row in the tab is New or RefundRequested only.
        Assert.All(paged.Items, o => Assert.Contains(o.Status, new[] { "new", "refund-requested" }));
    }

    [Fact]
    public async Task GetOrderCounts_ReflectsCreatedOrders()
    {
        var ct = TestContext.Current.CancellationToken;

        var before = await (await _client.GetAsync("/api/orders/counts", ct))
            .Content.ReadFromJsonAsync<OrderCountsDto>(ct);
        Assert.NotNull(before);

        await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(NextNumber()), ct);
        await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(NextNumber()), ct);

        var after = await (await _client.GetAsync("/api/orders/counts", ct))
            .Content.ReadFromJsonAsync<OrderCountsDto>(ct);
        Assert.NotNull(after);

        Assert.Equal(before!.All + 2, after!.All);
        Assert.Equal(before.New + 2, after.New);
        Assert.Equal(before.NeedsAction + 2, after.NeedsAction);
    }

    [Fact]
    public async Task GetOrderByNumber_Unknown_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync("/api/orders/123456789", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PackOrder_FromNew_Returns200_AndEvictsOrdersTag()
    {
        var ct = TestContext.Current.CancellationToken;
        var number = NextNumber();
        await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(number), ct);

        var beforeCount = _cache.EvictedTags.Count(t => t == "orders");

        var response = await _seller.PostAsync($"/api/orders/{number}/pack", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrderDetailDto>(ct);
        Assert.Equal("packed", dto!.Status);

        Assert.True(_cache.EvictedTags.Count(t => t == "orders") > beforeCount,
            "Expected EvictByTagAsync(\"orders\", ...) after a successful pack.");
    }

    [Fact]
    public async Task PackShipDeliver_FullLifecycle_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;
        var number = NextNumber();
        await _seller.PostAsJsonAsync("/api/orders", NewOrderCommand(number), ct);

        var pack = await _seller.PostAsync($"/api/orders/{number}/pack", null, ct);
        Assert.Equal(HttpStatusCode.OK, pack.StatusCode);

        var ship = await _seller.PostAsJsonAsync($"/api/orders/{number}/ship",
            new ShipOrderCommand(number, "USPS", "USPS · 9405 lifecycle"), ct);
        Assert.Equal(HttpStatusCode.OK, ship.StatusCode);
        var shipped = await ship.Content.ReadFromJsonAsync<OrderDetailDto>(ct);
        Assert.Equal("shipped", shipped!.Status);

        var deliver = await _seller.PostAsync($"/api/orders/{number}/deliver", null, ct);
        Assert.Equal(HttpStatusCode.OK, deliver.StatusCode);
        var delivered = await deliver.Content.ReadFromJsonAsync<OrderDetailDto>(ct);
        Assert.Equal("delivered", delivered!.Status);
    }

    private sealed record PagedOrders(IReadOnlyList<OrderInboxRowDto> Items, int Total, int Page, int PageSize);
}
