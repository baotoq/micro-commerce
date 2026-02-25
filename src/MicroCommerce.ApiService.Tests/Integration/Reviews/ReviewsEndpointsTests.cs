using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetReviewsByProduct;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

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
        await ResetDatabase(typeof(ReviewsDbContext));
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
}
