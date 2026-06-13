using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Api.Endpoints;
using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Storefront;

public class StorefrontCheckoutTests(CatalogWebApplicationFactory factory)
    : IClassFixture<CatalogWebApplicationFactory>
{
    private static StorefrontOrderEndpoints.StorefrontCheckoutRequest Request(
        string? promo = null, int qty = 1, string sku = "MC-VS-001") =>
        new(
            CustomerName: "Bao Buyer",
            ShipLine1: "241 Telegraph Ave",
            ShipLine2: "Oakland, CA 94612",
            CityState: "Oakland, CA",
            ShippingMethod: "Standard",
            ShippingPaid: 8m,
            Tax: 6.46m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4242",
            PromoCode: promo,
            Lines: [new StorefrontLineInput(sku, qty)]);

    [Fact]
    public async Task Checkout_Anonymous_Is401()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_Buyer_CreatesOrder_WithDbPricing()
    {
        var client = factory.CreateBuyerClient("checkout-pricing@test.dev");
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(qty: 1),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.Equal(86m, dto!.Summary.Subtotal);             // seeded Persimmon vase price
        Assert.Equal("checkout-pricing@test.dev", dto.Customer.Email); // from token, not body
        Assert.Equal($"/api/orders/{dto.Number}/confirmation", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Checkout_WithWelcome10_PersistsDiscount()
    {
        var client = factory.CreateBuyerClient("checkout-promo@test.dev");
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(promo: "WELCOME10"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);
        Assert.Equal("WELCOME10", dto!.Summary.DiscountCode);
        Assert.Equal(10m, dto.Summary.DiscountAmount);
        Assert.Equal(86m - 10m + 8m + 6.46m, dto.Summary.Paid);
    }

    [Fact]
    public async Task Checkout_InvalidPromo_Is409WithCode()
    {
        var client = factory.CreateBuyerClient();
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(promo: "NOPE-99"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsLite>(TestContext.Current.CancellationToken);
        Assert.Equal("PROMO_INVALID", problem!.Title);
    }

    [Fact]
    public async Task Checkout_OverStock_Is409()
    {
        var client = factory.CreateBuyerClient();
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(qty: 100_000),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsLite>(TestContext.Current.CancellationToken);
        Assert.Equal("INSUFFICIENT_STOCK", problem!.Title);
    }

    [Fact]
    public async Task Confirmation_OwnOrder200_ForeignOrder404()
    {
        var owner = factory.CreateBuyerClient("confirm-owner@test.dev");
        var created = await owner.PostAsJsonAsync("/api/orders/checkout", Request(),
            TestContext.Current.CancellationToken);
        var dto = await created.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);

        var own = await owner.GetAsync($"/api/orders/{dto!.Number}/confirmation",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        var stranger = factory.CreateBuyerClient("someone-else@test.dev");
        var foreign = await stranger.GetAsync($"/api/orders/{dto.Number}/confirmation",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
    }

    [Fact]
    public async Task SellerOrderPost_StillRejectsRolelessBuyer()
    {
        var buyer = factory.CreateBuyerClient();
        // Seller CreateOrderCommand body — shape irrelevant, the 403 happens at the policy.
        var response = await buyer.PostAsJsonAsync("/api/orders", new { number = 1 },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record ProblemDetailsLite(string Title, string? Detail);
}
