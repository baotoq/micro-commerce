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
}
