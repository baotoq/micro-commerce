using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews;
using MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetReviewsByProduct;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Builders;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.ApiService.Tests.Integration.Reviews;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class ReviewsEndpointsTests : IntegrationTestBase
{
    public ReviewsEndpointsTests(ApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(ReviewsDbContext), typeof(OrderingDbContext));
    }

    [Fact]
    public async Task GetProductReviews_NoReviews_ReturnsEmptyList()
    {
        // Arrange - Use guest client (public endpoint, no auth required)
        HttpClient client = CreateGuestClient();
        Guid productId = Guid.NewGuid();

        // Act
        ReviewListDto? result = await client.GetFromJsonAsync<ReviewListDto>(
            $"/api/reviews/products/{productId}");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CanReview_WithPurchase_ReturnsTrueForHasPurchased()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        await SeedPaidOrder(userId, productId);

        // Act
        CanReviewDto? result = await client.GetFromJsonAsync<CanReviewDto>(
            $"/api/reviews/products/{productId}/can-review");

        // Assert
        result.Should().NotBeNull();
        result!.HasPurchased.Should().BeTrue();
        result.HasReviewed.Should().BeFalse();
    }

    [Fact]
    public async Task CanReview_WithoutPurchase_ReturnsFalseForHasPurchased()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        CanReviewDto? result = await client.GetFromJsonAsync<CanReviewDto>(
            $"/api/reviews/products/{productId}/can-review");

        // Assert
        result.Should().NotBeNull();
        result!.HasPurchased.Should().BeFalse();
        result.HasReviewed.Should().BeFalse();
    }

    [Fact]
    public async Task CreateReview_WithVerifiedPurchase_Returns201()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        await SeedPaidOrder(userId, productId);

        CreateReviewRequest request = new(5, "Excellent product, highly recommend!");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/reviews/products/{productId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateReviewResult? result = await response.Content.ReadFromJsonAsync<CreateReviewResult>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateReview_WithoutPurchase_Returns400()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        CreateReviewRequest request = new(4, "This is a review without purchasing first");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/reviews/products/{productId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMyReview_AfterCreating_ReturnsReview()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        await SeedPaidOrder(userId, productId);

        CreateReviewRequest createRequest = new(4, "Great product, would buy again!");
        await client.PostAsJsonAsync($"/api/reviews/products/{productId}", createRequest);

        // Act
        ReviewDto? review = await client.GetFromJsonAsync<ReviewDto>(
            $"/api/reviews/products/{productId}/mine");

        // Assert
        review.Should().NotBeNull();
        review!.Rating.Should().Be(4);
        review.Text.Should().Be("Great product, would buy again!");
        review.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetMyReview_NoReview_Returns404()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"/api/reviews/products/{productId}/mine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReview_ByOwner_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        await SeedPaidOrder(userId, productId);

        CreateReviewRequest createRequest = new(3, "Decent product, nothing special");
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            $"/api/reviews/products/{productId}", createRequest);
        CreateReviewResult? created = await createResponse.Content.ReadFromJsonAsync<CreateReviewResult>();

        UpdateReviewRequest updateRequest = new(5, "Changed my mind, this product is amazing!");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/reviews/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        ReviewDto? review = await client.GetFromJsonAsync<ReviewDto>(
            $"/api/reviews/products/{productId}/mine");
        review!.Rating.Should().Be(5);
        review.Text.Should().Be("Changed my mind, this product is amazing!");
    }

    [Fact]
    public async Task UpdateReview_ByNonOwner_ReturnsError()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        HttpClient ownerClient = CreateAuthenticatedClient(ownerId);
        HttpClient otherClient = CreateAuthenticatedClient(otherUserId);

        await SeedPaidOrder(ownerId, productId);

        CreateReviewRequest createRequest = new(4, "This is my review and nobody else's");
        HttpResponseMessage createResponse = await ownerClient.PostAsJsonAsync(
            $"/api/reviews/products/{productId}", createRequest);
        CreateReviewResult? created = await createResponse.Content.ReadFromJsonAsync<CreateReviewResult>();

        UpdateReviewRequest updateRequest = new(1, "Trying to overwrite someone else's review");

        // Act
        HttpResponseMessage response = await otherClient.PutAsJsonAsync(
            $"/api/reviews/{created!.Id}", updateRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteReview_ByOwner_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        await SeedPaidOrder(userId, productId);

        CreateReviewRequest createRequest = new(2, "Not great, removing my review soon");
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            $"/api/reviews/products/{productId}", createRequest);
        CreateReviewResult? created = await createResponse.Content.ReadFromJsonAsync<CreateReviewResult>();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/reviews/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        HttpResponseMessage getResponse = await client.GetAsync(
            $"/api/reviews/products/{productId}/mine");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteReview_ByNonOwner_ReturnsError()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        HttpClient ownerClient = CreateAuthenticatedClient(ownerId);
        HttpClient otherClient = CreateAuthenticatedClient(otherUserId);

        await SeedPaidOrder(ownerId, productId);

        CreateReviewRequest createRequest = new(5, "My precious review that nobody should delete");
        HttpResponseMessage createResponse = await ownerClient.PostAsJsonAsync(
            $"/api/reviews/products/{productId}", createRequest);
        CreateReviewResult? created = await createResponse.Content.ReadFromJsonAsync<CreateReviewResult>();

        // Act
        HttpResponseMessage response = await otherClient.DeleteAsync($"/api/reviews/{created!.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    /// <summary>
    /// Seeds an order in Paid status for the given user and product,
    /// so that the verified-purchase check passes for review operations.
    /// </summary>
    private async Task SeedPaidOrder(Guid userId, Guid productId)
    {
        using IServiceScope scope = CreateScope();
        var orderingContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        var order = new OrderBuilder()
            .WithBuyerId(userId)
            .WithItem(productId, "Test Product", 49.99m, null, 1)
            .Build();

        order.MarkAsPaid();

        orderingContext.Orders.Add(order);
        await orderingContext.SaveChangesAsync();
    }
}
