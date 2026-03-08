using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetUserWishlist;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Wishlists;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class WishlistsEndpointsTests : IntegrationTestBase
{
    public WishlistsEndpointsTests(ApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(WishlistsDbContext));
    }

    [Fact]
    public async Task GetWishlist_EmptyWishlist_ReturnsEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        List<WishlistItemDto>? result = await client.GetFromJsonAsync<List<WishlistItemDto>>(
            "/api/wishlist/");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddToWishlist_ValidProduct_Returns201()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.PostAsync(
            $"/api/wishlist/{productId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RemoveFromWishlist_ExistingItem_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Add first
        await client.PostAsync($"/api/wishlist/{productId}", null);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/wishlist/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetWishlistCount_EmptyWishlist_ReturnsZero()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        int count = await client.GetFromJsonAsync<int>("/api/wishlist/count");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetWishlistCount_AfterAdds_ReturnsCorrectCount()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        await client.PostAsync($"/api/wishlist/{Guid.NewGuid()}", null);
        await client.PostAsync($"/api/wishlist/{Guid.NewGuid()}", null);
        await client.PostAsync($"/api/wishlist/{Guid.NewGuid()}", null);

        // Act
        int count = await client.GetFromJsonAsync<int>("/api/wishlist/count");

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetWishlistProductIds_EmptyWishlist_ReturnsEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        List<Guid>? productIds = await client.GetFromJsonAsync<List<Guid>>(
            "/api/wishlist/product-ids");

        // Assert
        productIds.Should().NotBeNull();
        productIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWishlistProductIds_AfterAdds_ReturnsProductIds()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        Guid productId1 = Guid.NewGuid();
        Guid productId2 = Guid.NewGuid();

        await client.PostAsync($"/api/wishlist/{productId1}", null);
        await client.PostAsync($"/api/wishlist/{productId2}", null);

        // Act
        List<Guid>? productIds = await client.GetFromJsonAsync<List<Guid>>(
            "/api/wishlist/product-ids");

        // Assert
        productIds.Should().NotBeNull();
        productIds.Should().HaveCount(2);
        productIds.Should().Contain(productId1);
        productIds.Should().Contain(productId2);
    }
}
