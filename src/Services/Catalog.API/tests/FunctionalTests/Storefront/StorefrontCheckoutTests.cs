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

        // Review finding 5: the checkout response DTO is built from the in-memory order entity,
        // so it can't prove the discount columns actually persisted. Re-read via the confirmation
        // GET, which loads the order back from Postgres through GetOrderByNumberQuery, and assert
        // the discount + totals survive the write+reload round trip.
        var confirmation = await client.GetAsync($"/api/orders/{dto.Number}/confirmation",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);
        var reloaded = await confirmation.Content.ReadFromJsonAsync<OrderDetailDto>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(reloaded);
        Assert.Equal("WELCOME10", reloaded!.Summary.DiscountCode);
        Assert.Equal(10m, reloaded.Summary.DiscountAmount);
        Assert.Equal(86m, reloaded.Summary.Subtotal);
        Assert.Equal(86m - 10m + 8m + 6.46m, reloaded.Summary.Paid);
        var line = Assert.Single(reloaded.Lines);
        Assert.Equal("MC-VS-001", line.Sku);
        Assert.Equal(1, line.Qty);
        Assert.Equal(86m, line.UnitPrice);
    }

    [Fact]
    public async Task Checkout_RecomputesShippingAndTax_IgnoringClientValues()
    {
        // Review finding 4: a buyer POSTing ShippingPaid:0 / Tax:0 must not underpay. The server
        // recomputes shipping from the method ($8 Standard) and tax at 8.5% of the discounted
        // subtotal ($86 -> $7.31), regardless of what the request claims.
        var client = factory.CreateBuyerClient("checkout-money@test.dev");
        var tampered = Request() with { ShippingPaid = 0m, Tax = 0m };
        var response = await client.PostAsJsonAsync("/api/orders/checkout", tampered,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);
        Assert.Equal(8m, dto!.Summary.Shipping);
        Assert.Equal(7.31m, dto.Summary.Tax);
        Assert.Equal(86m + 8m + 7.31m, dto.Summary.Paid);
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
        // Confirmation must surface lines + totals (spec §4), read back from Postgres.
        var ownBody = await own.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(ownBody);
        Assert.Equal(dto.Number, ownBody!.Number);
        Assert.Equal(86m, ownBody.Summary.Subtotal);
        Assert.Equal("MC-VS-001", Assert.Single(ownBody.Lines).Sku);

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
