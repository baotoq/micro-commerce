using Microsoft.EntityFrameworkCore;
using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;
using Xunit;
using VerifyTests;

namespace MicroCommerce.CartService.Api.IntegrationTests.Endpoints;

public class CartEndpointsTests : TestBase
{
    [Fact]
    public async Task GetCart_ShouldReturnCart_WhenCartExists()
    {
        // Arrange
        var cart = Cart.Create();
        await SeedContext.Carts.AddAsync(cart);
        await SeedContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/carts/{cart.Id}");

        // Assert
        await Verify(response);
    }

    [Fact]
    public async Task GetCart_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        var nonExistentCartId = CartId.New();

        // Act
        var response = await Client.GetAsync($"/api/carts/{nonExistentCartId}");

        // Assert
        await Verify(response);
    }

    [Fact]
    public async Task CreateCart_ShouldCreateNewCart()
    {
        // Act
        var response = await Client.PostAsync("/api/carts", null);

        // Assert
        await Verify(response);

        var cart = await SeedContext.Carts.FirstOrDefaultAsync();
        await Verify(cart);
    }

    [Fact]
    public async Task RemoveProductFromCart_ShouldRemoveProduct_WhenProductExists()
    {
        // Arrange
        var cart = Cart.Create();
        var cartItemId = CartItemId.New();
        cart.AddItem(cartItemId, 1, new Money(10.0m));
        await SeedContext.Carts.AddAsync(cart);
        await SeedContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/carts/{cart.Id}/items/{cartItemId}");

        // Assert
        await Verify(response);

        var updatedCart = await SeedContext.Carts
            .Include(c => c.Items)
            .FirstAsync(c => c.Id == cart.Id);
        await Verify(updatedCart);
    }

    [Fact]
    public async Task RemoveProductFromCart_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        var nonExistentCartId = CartId.New();
        var cartItemId = CartItemId.New();

        // Act
        var response = await Client.DeleteAsync($"/api/carts/{nonExistentCartId}/items/{cartItemId}");

        // Assert
        await Verify(response);
    }

    [Fact]
    public async Task RemoveProductFromCart_ShouldReturnBadRequest_WhenItemDoesNotExist()
    {
        // Arrange
        var cart = Cart.Create();
        await SeedContext.Carts.AddAsync(cart);
        await SeedContext.SaveChangesAsync();
        var nonExistentItemId = CartItemId.New();

        // Act
        var response = await Client.DeleteAsync($"/api/carts/{cart.Id}/items/{nonExistentItemId}");

        // Assert
        await Verify(response);
    }
}
