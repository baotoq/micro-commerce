using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Coupons;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Ordering;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class CheckoutWithCouponTests : IntegrationTestBase
{
    private readonly HttpClient _client;
    private readonly Guid _buyerId;

    public CheckoutWithCouponTests(ApiWebApplicationFactory factory) : base(factory)
    {
        _client = factory.CreateClient();
        _buyerId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Add("Cookie", $"buyer_id={_buyerId}");
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(OrderingDbContext), typeof(CouponsDbContext));
    }

    private async Task<Guid> CreateCouponAsync(
        string code,
        string discountType = "Percentage",
        decimal discountValue = 10m,
        decimal? minOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        int? usagePerUser = null,
        DateTimeOffset? validUntil = null,
        bool active = true)
    {
        CreateCouponRequest request = new(
            code,
            "Test coupon",
            discountType,
            discountValue,
            DateTimeOffset.UtcNow.AddDays(-1),
            validUntil,
            minOrderAmount,
            maxDiscountAmount,
            usageLimit,
            usagePerUser,
            null,
            null);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/coupons", request);
        Guid id = await response.Content.ReadFromJsonAsync<Guid>();

        if (!active)
            await _client.PatchAsJsonAsync($"/api/coupons/{id}/status", new ToggleCouponStatusRequest(false));

        return id;
    }

    private CheckoutRequest BuildCheckoutRequest(string? couponCode = null) =>
        new(
            "customer@example.com",
            new ShippingAddressRequest(
                "John Doe",
                "john@example.com",
                "123 Main St",
                "Seattle",
                "WA",
                "98101"),
            [
                new OrderItemRequest(Guid.NewGuid(), "Product A", 50m, null, 2),
                new OrderItemRequest(Guid.NewGuid(), "Product B", 100m, null, 1)
            ],
            couponCode);

    [Fact]
    public async Task Checkout_WithoutCoupon_CreatesOrderWithZeroDiscount()
    {
        // Arrange
        CheckoutRequest request = BuildCheckoutRequest();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = await response.Content.ReadFromJsonAsync<Guid>();
        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");

        order.Should().NotBeNull();
        order!.DiscountAmount.Should().Be(0m);
        order.CouponCode.Should().BeNull();
        // subtotal = 50*2 + 100 = 200; tax = 200 * 0.08 = 16; shipping = 5.99; total = 221.99
        order.Subtotal.Should().Be(200m);
        order.Total.Should().Be(221.99m);
    }

    [Fact]
    public async Task Checkout_WithValidPercentageCoupon_AppliesDiscountCorrectly()
    {
        // Arrange - 10% off coupon
        await CreateCouponAsync("SAVE10");
        CheckoutRequest request = BuildCheckoutRequest("SAVE10");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = await response.Content.ReadFromJsonAsync<Guid>();
        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");

        order.Should().NotBeNull();
        order!.CouponCode.Should().Be("SAVE10");
        // subtotal = 200, discount = 200 * 10% = 20
        order.DiscountAmount.Should().Be(20m);
        // tax = (200 - 20) * 0.08 = 14.40; total = 200 - 20 + 5.99 + 14.40 = 200.39
        order.Tax.Should().Be(14.40m);
        order.Total.Should().Be(200.39m);
    }

    [Fact]
    public async Task Checkout_WithValidFixedAmountCoupon_AppliesDiscountCorrectly()
    {
        // Arrange - $15 fixed discount
        await CreateCouponAsync("FLAT15", discountType: "FixedAmount", discountValue: 15m);
        CheckoutRequest request = BuildCheckoutRequest("FLAT15");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = await response.Content.ReadFromJsonAsync<Guid>();
        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");

        order.Should().NotBeNull();
        order!.CouponCode.Should().Be("FLAT15");
        order.DiscountAmount.Should().Be(15m);
        // tax = (200 - 15) * 0.08 = 14.80; total = 200 - 15 + 5.99 + 14.80 = 205.79
        order.Tax.Should().Be(14.80m);
        order.Total.Should().Be(205.79m);
    }

    [Fact]
    public async Task Checkout_WithInvalidCouponCode_ReturnsError()
    {
        // Arrange - coupon does not exist
        CheckoutRequest request = BuildCheckoutRequest("NOTEXIST");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Checkout_WithExpiredCoupon_ReturnsError()
    {
        // Arrange - expired coupon
        await CreateCouponAsync(
            "EXPIRED",
            validUntil: DateTimeOffset.UtcNow.AddDays(-1));
        CheckoutRequest request = BuildCheckoutRequest("EXPIRED");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Checkout_WithInactiveCoupon_ReturnsError()
    {
        // Arrange
        await CreateCouponAsync("INACTIVE", active: false);
        CheckoutRequest request = BuildCheckoutRequest("INACTIVE");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Checkout_WithUsageLimitedCoupon_IncrementsUsageCount()
    {
        // Arrange - 2 uses allowed
        await CreateCouponAsync("LIMITED", usageLimit: 2);

        HttpClient client2 = Factory.CreateClient();
        client2.DefaultRequestHeaders.Add("Cookie", $"buyer_id={Guid.NewGuid()}");

        // Act - first use
        await _client.PostAsJsonAsync("/api/ordering/checkout", BuildCheckoutRequest("LIMITED"));
        // Second use (different buyer)
        HttpResponseMessage response2 = await client2.PostAsJsonAsync("/api/ordering/checkout", BuildCheckoutRequest("LIMITED"));

        // Third use (should fail - limit reached)
        HttpClient client3 = Factory.CreateClient();
        client3.DefaultRequestHeaders.Add("Cookie", $"buyer_id={Guid.NewGuid()}");
        HttpResponseMessage response3 = await client3.PostAsJsonAsync("/api/ordering/checkout", BuildCheckoutRequest("LIMITED"));

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
        response3.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Checkout_WithPercentageCappedByMaxDiscount_AppliesCap()
    {
        // Arrange - 50% off, capped at $30
        await CreateCouponAsync("CAPPED50", discountValue: 50m, maxDiscountAmount: 30m);
        CheckoutRequest request = BuildCheckoutRequest("CAPPED50");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = await response.Content.ReadFromJsonAsync<Guid>();
        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");

        // subtotal = 200, 50% = 100 but capped at 30
        order!.DiscountAmount.Should().Be(30m);
    }

    [Fact]
    public async Task Checkout_WithFixedDiscountExceedingSubtotal_CapsAtSubtotal()
    {
        // Arrange - $500 fixed discount on $200 order
        await CreateCouponAsync("BIGDISCOUNT", discountType: "FixedAmount", discountValue: 500m);
        CheckoutRequest request = BuildCheckoutRequest("BIGDISCOUNT");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = await response.Content.ReadFromJsonAsync<Guid>();
        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");

        // discount capped at subtotal
        order!.DiscountAmount.Should().Be(200m);
        order.Total.Should().BeGreaterThanOrEqualTo(0m);
    }
}
