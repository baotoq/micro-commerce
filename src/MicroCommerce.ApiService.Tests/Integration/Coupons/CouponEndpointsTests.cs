using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Coupons;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.ValidateCoupon;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Coupons;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class CouponEndpointsTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    public CouponEndpointsTests(ApiWebApplicationFactory factory) : base(factory)
    {
        _client = factory.CreateClient();
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(CouponsDbContext));
    }

    private static CreateCouponRequest BuildCreateRequest(
        string code = "TEST10",
        string discountType = "Percentage",
        decimal discountValue = 10m,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null,
        decimal? minOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        int? usagePerUser = null) =>
        new(
            code,
            "Test coupon",
            discountType,
            discountValue,
            validFrom ?? DateTimeOffset.UtcNow.AddDays(-1),
            validUntil,
            minOrderAmount,
            maxDiscountAmount,
            usageLimit,
            usagePerUser,
            null,
            null);

    [Fact]
    public async Task CreateCoupon_ValidRequest_Returns201()
    {
        // Arrange
        CreateCouponRequest request = BuildCreateRequest();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateCoupon_DuplicateCode_Returns500()
    {
        // Arrange
        CreateCouponRequest request = BuildCreateRequest("DUPLICATE");
        await _client.PostAsJsonAsync("/api/coupons", request);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCoupon_InvalidPercentageOver100_Returns422()
    {
        // Arrange - 150% discount is invalid
        CreateCouponRequest request = BuildCreateRequest(discountType: "Percentage", discountValue: 150m);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetCoupons_ReturnsPagedList()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("FIRST"));
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("SECOND"));

        // Act
        CouponListDto? result = await _client.GetFromJsonAsync<CouponListDto>("/api/coupons");

        // Assert
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCouponById_ExistingCoupon_ReturnsCoupon()
    {
        // Arrange
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("BYID"));
        Guid id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        CouponDto? coupon = await _client.GetFromJsonAsync<CouponDto>($"/api/coupons/{id}");

        // Assert
        coupon.Should().NotBeNull();
        coupon!.Code.Should().Be("BYID");
        coupon.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCouponById_NotFound_Returns404()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/coupons/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCoupon_ValidRequest_Returns204()
    {
        // Arrange
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("UPDATE"));
        Guid id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        UpdateCouponRequest updateRequest = new(
            "Updated description",
            "FixedAmount",
            5m,
            DateTimeOffset.UtcNow.AddDays(-1),
            null, null, null, null, null, null, null);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/coupons/{id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        CouponDto? updated = await _client.GetFromJsonAsync<CouponDto>($"/api/coupons/{id}");
        updated!.DiscountType.Should().Be("FixedAmount");
        updated.DiscountValue.Should().Be(5m);
        updated.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task ToggleCouponStatus_Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("TOGGLE"));
        Guid id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - deactivate
        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/coupons/{id}/status",
            new ToggleCouponStatusRequest(false));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        CouponDto? coupon = await _client.GetFromJsonAsync<CouponDto>($"/api/coupons/{id}");
        coupon!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCoupon_ExistingCoupon_Returns204()
    {
        // Arrange
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("DELETE"));
        Guid id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/coupons/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        HttpResponseMessage getResponse = await _client.GetAsync($"/api/coupons/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ValidateCoupon_ValidCode_ReturnsDiscountAmount()
    {
        // Arrange - 10% off, no min order
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("VALID10", discountValue: 10m));

        // Act
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("VALID10", 100m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result.Should().NotBeNull();
        result!.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(10m);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateCoupon_InvalidCode_ReturnsError()
    {
        // Act
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("NOTEXIST", 100m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result!.IsValid.Should().BeFalse();
        result.DiscountAmount.Should().Be(0);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateCoupon_InactiveCoupon_ReturnsError()
    {
        // Arrange
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("INACTIVE"));
        Guid id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        await _client.PatchAsJsonAsync($"/api/coupons/{id}/status", new ToggleCouponStatusRequest(false));

        // Act
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("INACTIVE", 100m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result!.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not active");
    }

    [Fact]
    public async Task ValidateCoupon_MinOrderAmountNotMet_ReturnsError()
    {
        // Arrange - requires $50 minimum
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("MINORDER", minOrderAmount: 50m));

        // Act - only $30 subtotal
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("MINORDER", 30m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result!.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Minimum order");
    }

    [Fact]
    public async Task ValidateCoupon_PercentageCappedByMaxDiscountAmount()
    {
        // Arrange - 50% off, capped at $10
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest(
            "CAPPED",
            discountType: "Percentage",
            discountValue: 50m,
            maxDiscountAmount: 10m));

        // Act - $100 subtotal → 50% = $50, but capped at $10
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("CAPPED", 100m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result!.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(10m);
    }

    [Fact]
    public async Task ValidateCoupon_FixedAmountCappedAtSubtotal()
    {
        // Arrange - $50 fixed discount on $30 order
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest(
            "FIXED50",
            discountType: "FixedAmount",
            discountValue: 50m));

        // Act
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("FIXED50", 30m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result!.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(30m); // capped at subtotal
    }

    [Fact]
    public async Task ValidateCoupon_ExpiredCoupon_ReturnsError()
    {
        // Arrange - expired yesterday
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest(
            "EXPIRED",
            validFrom: DateTimeOffset.UtcNow.AddDays(-10),
            validUntil: DateTimeOffset.UtcNow.AddDays(-1)));

        // Act
        ValidateCouponResult? result = await _client.PostAsJsonAsync(
            "/api/coupons/validate",
            new ValidateCouponRequest("EXPIRED", 100m))
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<ValidateCouponResult>())
            .Unwrap();

        // Assert
        result!.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task GetCoupons_FilterByIsActive_ReturnsOnlyActive()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("ACTIVE1"));
        HttpResponseMessage resp2 = await _client.PostAsJsonAsync("/api/coupons", BuildCreateRequest("INACTIVE2"));
        Guid id2 = await resp2.Content.ReadFromJsonAsync<Guid>();
        await _client.PatchAsJsonAsync($"/api/coupons/{id2}/status", new ToggleCouponStatusRequest(false));

        // Act
        CouponListDto? result = await _client.GetFromJsonAsync<CouponListDto>("/api/coupons?isActive=true");

        // Assert
        result!.Items.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());
        result.Items.Should().Contain(c => c.Code == "ACTIVE1");
        result.Items.Should().NotContain(c => c.Code == "INACTIVE2");
    }
}
