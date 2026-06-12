using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Customers.Commands;
using MicroCommerce.Catalog.Application.Marketing.Commands;
using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Promotions.Commands;

namespace MicroCommerce.Catalog.FunctionalTests.Auth;

/// <summary>
/// End-to-end authorization matrix over one representative write route per group, driven through the
/// REAL JwtBearer handler + REAL seller policy (tokens self-minted by <see cref="TestTokens"/>; the
/// factory pre-seeds matching validation parameters — no Keycloak container). Each row asserts:
/// no token → 401, authenticated-without-seller → 403, seller → 2xx. Plus wrong-audience → 401 and the
/// public read endpoint stays anonymous → 200.
/// </summary>
public class WriteEndpointAuthTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private static string Unique() => $"FN-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    private HttpClient AnonymousClient() => factory.CreateClient();

    private HttpClient ClientWithToken(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ---- request-body factories: valid payloads so an authorized seller gets a clean 2xx ----

    private static (string Url, object Body) ProductCreate() =>
        ("/api/products",
            new CreateProductCommand(Unique(), "Auth Test Vase", "Vessels", 49.99m, 10, "active",
                PhotoUrls: ["https://example.com/p.jpg"]));

    private static (string Url, object Body) PromotionCreate() =>
        ("/api/promotions",
            new CreatePromotionCommand(Unique(), "auth test", "percentage", 20, null, null, null, null, false));

    private static (string Url, object Body) CustomerCreate() =>
        ("/api/customers",
            new CreateCustomerCommand("Auth Test", $"{Unique().ToLowerInvariant()}@example.com", "Portland, OR"));

    private static (string Url, object Body) MarketingCreate() =>
        ("/api/marketing/campaigns",
            // Clock is bound server-side by the endpoint; the body value is ignored.
            new CreateCampaignCommand("Auth Test Campaign", "Subject line", "Preview text",
                "all", "basic", null, false, null, Clock: null!));

    private static (string Url, object Body) OrderCreate() =>
        ("/api/orders",
            new CreateOrderCommand(
                Number: Random.Shared.Next(900_000, 999_999),
                PlacedAt: DateTimeOffset.UtcNow,
                CustomerName: "Auth Test",
                CustomerEmail: $"{Unique().ToLowerInvariant()}@example.com",
                CityState: "Portland, OR",
                ShipLine1: "1 Test St",
                ShipLine2: "",
                BillSameAsShip: true,
                ItemsSummary: "1 item",
                ShippingMethod: "standard",
                ShippingPaid: 5m,
                Tax: 1m,
                FeePct: 2.9m,
                PaymentBrand: "visa",
                PaymentLastFour: "4242",
                LabelCarrier: null,
                LabelCost: 0m,
                LabelWeightLabel: null,
                InternalNote: "",
                Lines:
                [
                    new CreateOrderLineInput("SKU-AUTH-1", "Auth Item", "natural", 1, 10m, "awaiting", null, null)
                ]));

    public static IEnumerable<(string, Func<(string, object)>)> WriteRoutes()
    {
        yield return ("products", ProductCreate);
        yield return ("promotions", PromotionCreate);
        yield return ("customers", CustomerCreate);
        yield return ("marketing", MarketingCreate);
        yield return ("orders", OrderCreate);
    }

    public static IEnumerable<object[]> WriteRouteNames() =>
        WriteRoutes().Select(r => new object[] { r.Item1 });

    private static (string Url, object Body) BodyFor(string group) =>
        WriteRoutes().First(r => r.Item1 == group).Item2();

    [Theory]
    [MemberData(nameof(WriteRouteNames))]
    public async Task WriteRoute_NoToken_Returns401(string group)
    {
        var ct = TestContext.Current.CancellationToken;
        var (url, body) = BodyFor(group);

        var response = await AnonymousClient().PostAsJsonAsync(url, body, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(WriteRouteNames))]
    public async Task WriteRoute_TokenWithoutSellerRole_Returns403(string group)
    {
        var ct = TestContext.Current.CancellationToken;
        var (url, body) = BodyFor(group);
        var client = ClientWithToken(TestTokens.Mint(["buyer"]));

        var response = await client.PostAsJsonAsync(url, body, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(WriteRouteNames))]
    public async Task WriteRoute_TokenWithSellerRole_Returns2xx(string group)
    {
        var ct = TestContext.Current.CancellationToken;
        var (url, body) = BodyFor(group);
        var client = factory.CreateSellerClient();

        var response = await client.PostAsJsonAsync(url, body, ct);

        Assert.True(
            (int)response.StatusCode is >= 200 and < 300,
            $"Expected 2xx for an authorized seller on {url}, got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task WriteRoute_WrongAudience_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var (url, body) = ProductCreate();
        var client = ClientWithToken(TestTokens.Mint(["seller"], audience: "some-other-api"));

        var response = await client.PostAsJsonAsync(url, body, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PublicReadEndpoint_NoToken_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await AnonymousClient().GetAsync("/api/products", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
