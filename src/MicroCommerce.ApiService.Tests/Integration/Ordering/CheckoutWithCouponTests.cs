using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Coupons.Domain.Entities;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.ApiService.Tests.Integration.Ordering;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class CheckoutWithCouponTests : IntegrationTestBase
{
    private readonly HttpClient _client;
    private readonly Guid _buyerId;

    public CheckoutWithCouponTests(ApiWebApplicationFactory factory) : base(factory)
    {
        _buyerId = Guid.NewGuid();
        _client = CreateGuestClient(_buyerId);
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(OrderingDbContext), typeof(CouponsDbContext));
    }

    [Fact]
    public async Task Checkout_WithValidFixedCoupon_AppliesDiscountCorrectly()
    {
        // Arrange - unit price 50, qty 2 = subtotal 100
        // Fixed coupon $10 off → discount=10, tax=(100-10)*0.08=7.20, total=100-10+5.99+7.20=103.19
        await CreateCouponAsync("FIXED10", DiscountType.FixedAmount, 10m);

        CheckoutRequest request = CheckoutRequestWith("FIXED10",
            new OrderItemRequest(Guid.NewGuid(), "Widget", 50m, null, 2));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order.Should().NotBeNull();
        order!.CouponCode.Should().Be("FIXED10");
        order.DiscountAmount.Should().Be(10m);
        order.Subtotal.Should().Be(100m);
        order.Tax.Should().Be(Math.Round((100m - 10m) * 0.08m, 2));
        order.Total.Should().Be(100m - 10m + 5.99m + order.Tax);
    }

    [Fact]
    public async Task Checkout_WithValidPercentageCoupon_AppliesDiscountCorrectly()
    {
        // Arrange - subtotal 200, 20% coupon → discount=40, tax=(200-40)*0.08=12.80, total=200-40+5.99+12.80=178.79
        await CreateCouponAsync("PCT20", DiscountType.Percentage, 20m);

        CheckoutRequest request = CheckoutRequestWith("PCT20",
            new OrderItemRequest(Guid.NewGuid(), "Gadget", 200m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order.Should().NotBeNull();
        order!.CouponCode.Should().Be("PCT20");
        order.DiscountAmount.Should().Be(40m);
        order.Subtotal.Should().Be(200m);
    }

    [Fact]
    public async Task Checkout_WithPercentageCouponAndMaxCap_CapsDiscount()
    {
        // Arrange - subtotal 200, 50% coupon capped at $50 → discount=50
        await CreateCouponAsync("CAP50", DiscountType.Percentage, 50m, maxDiscountAmount: 50m);

        CheckoutRequest request = CheckoutRequestWith("CAP50",
            new OrderItemRequest(Guid.NewGuid(), "Expensive Item", 200m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order.Should().NotBeNull();
        order!.DiscountAmount.Should().Be(50m); // capped at maxDiscountAmount
    }

    [Fact]
    public async Task Checkout_WithInvalidCoupon_CheckoutSucceedsWithNoDiscount()
    {
        // Arrange - no coupon created, just pass a random code
        CheckoutRequest request = CheckoutRequestWith("NONEXISTENT",
            new OrderItemRequest(Guid.NewGuid(), "Product", 100m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert - checkout succeeds but no discount applied
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order.Should().NotBeNull();
        order!.CouponCode.Should().BeNull();
        order.DiscountAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Checkout_WithExpiredCoupon_CheckoutSucceedsWithNoDiscount()
    {
        // Arrange - create expired coupon
        await CreateCouponAsync("EXPIRED", DiscountType.FixedAmount, 10m,
            validFrom: DateTimeOffset.UtcNow.AddDays(-10),
            validUntil: DateTimeOffset.UtcNow.AddDays(-1));

        CheckoutRequest request = CheckoutRequestWith("EXPIRED",
            new OrderItemRequest(Guid.NewGuid(), "Product", 100m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert - no discount for expired coupon
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order.Should().NotBeNull();
        order!.CouponCode.Should().BeNull();
        order.DiscountAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Checkout_WithUsageLimitedCoupon_IncrementsTimesUsed()
    {
        // Arrange - coupon with limit of 5
        await CreateCouponAsync("LIMITED", DiscountType.FixedAmount, 5m, usageLimit: 5);

        CheckoutRequest request = CheckoutRequestWith("LIMITED",
            new OrderItemRequest(Guid.NewGuid(), "Product", 50m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify TimesUsed incremented in DB
        using IServiceScope scope = CreateScope();
        CouponsDbContext couponsDb = scope.ServiceProvider.GetRequiredService<CouponsDbContext>();
        Coupon? coupon = await couponsDb.Coupons.FindAsync(
            await GetCouponIdByCodeAsync("LIMITED"));
        coupon.Should().NotBeNull();
        coupon!.TimesUsed.Should().Be(1);
    }

    [Fact]
    public async Task Checkout_WithExhaustedCoupon_CheckoutSucceedsWithNoDiscount()
    {
        // Arrange - coupon with limit of 1, TimesUsed already at 1
        await CreateCouponAsync("USED_UP", DiscountType.FixedAmount, 10m, usageLimit: 1,
            timesUsed: 1);

        CheckoutRequest request = CheckoutRequestWith("USED_UP",
            new OrderItemRequest(Guid.NewGuid(), "Product", 100m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert - checkout succeeds but no discount
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order!.DiscountAmount.Should().Be(0m);
        order.CouponCode.Should().BeNull();
    }

    [Fact]
    public async Task Checkout_WithMinOrderAmountNotMet_CheckoutSucceedsWithNoDiscount()
    {
        // Arrange - coupon requires $100 min, but subtotal is only $50
        await CreateCouponAsync("MINORDER", DiscountType.FixedAmount, 10m, minOrderAmount: 100m);

        CheckoutRequest request = CheckoutRequestWith("MINORDER",
            new OrderItemRequest(Guid.NewGuid(), "Cheap Product", 50m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order!.DiscountAmount.Should().Be(0m);
        order.CouponCode.Should().BeNull();
    }

    [Fact]
    public async Task Checkout_WithFixedDiscountExceedingSubtotal_CapsDiscountAtSubtotal()
    {
        // Arrange - fixed $200 off, but subtotal is only $50
        await CreateCouponAsync("BIGFIX", DiscountType.FixedAmount, 200m);

        CheckoutRequest request = CheckoutRequestWith("BIGFIX",
            new OrderItemRequest(Guid.NewGuid(), "Product", 50m, null, 1));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        // discount capped at subtotal (50), so total = 50 - 50 + 5.99 + 0 = 5.99
        order!.DiscountAmount.Should().Be(50m);
        order.Total.Should().Be(50m - 50m + 5.99m + 0m);
    }

    [Fact]
    public async Task Checkout_WithoutCoupon_WorksAsBeforeBackwardCompatible()
    {
        // Arrange - no coupon code in request
        CheckoutRequest request = new(
            "customer@example.com",
            new ShippingAddressRequest("John Doe", "john@example.com", "123 Main St", "Seattle", "WA", "98101"),
            new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), "Product A", 100m, null, 1)
            });

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/ordering/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid orderId = (await response.Content.ReadFromJsonAsync<Guid>())!;

        OrderDto? order = await _client.GetFromJsonAsync<OrderDto>($"/api/ordering/orders/{orderId}");
        order.Should().NotBeNull();
        order!.CouponCode.Should().BeNull();
        order.DiscountAmount.Should().Be(0m);
        order.Subtotal.Should().Be(100m);
        order.Tax.Should().Be(Math.Round(100m * 0.08m, 2));
        order.Total.Should().Be(100m + 5.99m + order.Tax);
    }

    // Helpers

    private CheckoutRequest CheckoutRequestWith(string couponCode, params OrderItemRequest[] items)
    {
        return new CheckoutRequest(
            "customer@example.com",
            new ShippingAddressRequest("John Doe", "john@example.com", "123 Main St", "Seattle", "WA", "98101"),
            items.ToList(),
            couponCode);
    }

    private async Task CreateCouponAsync(
        string code,
        DiscountType discountType,
        decimal discountValue,
        decimal? minOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null,
        int timesUsed = 0)
    {
        using IServiceScope scope = CreateScope();
        CouponsDbContext db = scope.ServiceProvider.GetRequiredService<CouponsDbContext>();

        Coupon coupon = Coupon.Create(
            code,
            $"Test coupon {code}",
            discountType,
            discountValue,
            validFrom ?? DateTimeOffset.UtcNow.AddDays(-1),
            validUntil,
            minOrderAmount,
            maxDiscountAmount,
            usageLimit);

        // Simulate timesUsed by calling IncrementUsage
        for (int i = 0; i < timesUsed; i++)
            coupon.IncrementUsage();

        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();
    }

    private async Task<CouponId> GetCouponIdByCodeAsync(string code)
    {
        using IServiceScope scope = CreateScope();
        CouponsDbContext db = scope.ServiceProvider.GetRequiredService<CouponsDbContext>();
        Coupon coupon = (await db.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant()))!;
        return coupon.Id;
    }
}
