using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;

namespace MicroCommerce.CartService.Domain.UnitTests.Entities;

public class CartTests
{
    [Fact]
    public async Task AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var cart = new Cart(CartId.New());
        var productId = Guid.NewGuid();
        var quantity = 2;
        var price = 10.0m;

        // Act
        cart.AddItem(productId, quantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public async Task AddItem_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var cart = new Cart(CartId.New());
        var productId = Guid.NewGuid();
        var initialQuantity = 1;
        var additionalQuantity = 2;
        var price = 10.0m;
        cart.AddItem(productId, initialQuantity, price);

        // Act
        cart.AddItem(productId, additionalQuantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var cart = new Cart(CartId.New());
        var productId = Guid.NewGuid();
        var invalidQuantity = 0;
        var price = 10.0m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(productId, invalidQuantity, price));
    }
}
