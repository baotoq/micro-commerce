using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Cart;
using MicroCommerce.ApiService.Features.Cart.Application.Commands.AddToCart;
using MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCart;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Cart;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class CartEndpointsTests
{
    private readonly HttpClient _client;
    private readonly Guid _buyerId;

    public CartEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _buyerId = Guid.NewGuid();

        // Set buyer_id cookie for cart identification
        _client.DefaultRequestHeaders.Add("Cookie", $"buyer_id={_buyerId}");
    }

    [Fact]
    public async Task GetCart_EmptyCart_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddToCart_ValidItem_Returns200()
    {
        // Arrange
        AddToCartRequest request = new(
            Guid.NewGuid(),
            "Test Product",
            49.99m,
            "https://example.com/image.jpg",
            2);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AddToCartResult? result = await response.Content.ReadFromJsonAsync<AddToCartResult>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCart_AfterAddItem_ReturnsItemInCart()
    {
        // Arrange - Add item first
        Guid productId = Guid.NewGuid();
        AddToCartRequest addRequest = new(
            productId,
            "Gaming Mouse",
            79.99m,
            null,
            1);

        await _client.PostAsJsonAsync("/api/cart/items", addRequest);

        // Act
        CartDto? cart = await _client.GetFromJsonAsync<CartDto>("/api/cart");

        // Assert
        cart.Should().NotBeNull();
        cart!.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(productId);
        cart.Items.First().ProductName.Should().Be("Gaming Mouse");
        cart.Items.First().Quantity.Should().Be(1);
    }

    [Fact]
    public async Task UpdateCartItem_ValidQuantity_ReturnsNoContent()
    {
        // Arrange - Add item first
        Guid productId = Guid.NewGuid();
        AddToCartRequest addRequest = new(
            productId,
            "Mechanical Keyboard",
            149.99m,
            null,
            1);

        await _client.PostAsJsonAsync("/api/cart/items", addRequest);

        // Get cart to find the item ID
        CartDto? cart = await _client.GetFromJsonAsync<CartDto>("/api/cart");
        Guid itemId = cart!.Items.First().Id;

        UpdateCartItemRequest updateRequest = new(3);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/cart/items/{itemId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        cart = await _client.GetFromJsonAsync<CartDto>("/api/cart");
        cart!.Items.First().Quantity.Should().Be(3);
    }

    [Fact]
    public async Task RemoveCartItem_ExistingItem_ReturnsNoContent()
    {
        // Arrange - Add two items
        AddToCartRequest request1 = new(
            Guid.NewGuid(),
            "Product A",
            10m,
            null,
            1);

        AddToCartRequest request2 = new(
            Guid.NewGuid(),
            "Product B",
            20m,
            null,
            1);

        await _client.PostAsJsonAsync("/api/cart/items", request1);
        await _client.PostAsJsonAsync("/api/cart/items", request2);

        // Get cart to find the first item ID
        CartDto? cart = await _client.GetFromJsonAsync<CartDto>("/api/cart");
        Guid firstItemId = cart!.Items.First().Id;

        // Act - Remove first item
        HttpResponseMessage response = await _client.DeleteAsync($"/api/cart/items/{firstItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify removal
        cart = await _client.GetFromJsonAsync<CartDto>("/api/cart");
        cart!.Items.Should().HaveCount(1);
        cart.Items.First().ProductName.Should().Be("Product B");
    }

    [Fact]
    public async Task GetCartItemCount_EmptyCart_ReturnsZero()
    {
        // Act
        int count = await _client.GetFromJsonAsync<int>("/api/cart/count");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetCartItemCount_AfterAddingItems_ReturnsCorrectCount()
    {
        // Arrange - Add two items with different quantities
        AddToCartRequest request1 = new(
            Guid.NewGuid(),
            "Product A",
            10m,
            null,
            3);

        AddToCartRequest request2 = new(
            Guid.NewGuid(),
            "Product B",
            20m,
            null,
            2);

        await _client.PostAsJsonAsync("/api/cart/items", request1);
        await _client.PostAsJsonAsync("/api/cart/items", request2);

        // Act
        int count = await _client.GetFromJsonAsync<int>("/api/cart/count");

        // Assert
        count.Should().Be(5); // 3 + 2
    }
}
