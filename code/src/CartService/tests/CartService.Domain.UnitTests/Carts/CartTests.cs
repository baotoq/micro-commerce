using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;
using MicroCommerce.CartService.Domain.UnitTests.TestHelpers.Builders;

namespace MicroCommerce.CartService.Domain.UnitTests.Carts;

public class CartTests
{
    [Fact]
    public async Task AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var productId = CartItemId.New();
        var quantity = 2;
        var price = new Money(10.0m);

        // Act
        cart.AddItem(productId, quantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public async Task AddItem_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var cart = new CartBuilder().WithItem(CartItemId.New(), 1, new Money(10.0m)).Build();
        var productId = CartItemId.New();
        var additionalQuantity = 2;
        var price = new Money(10.0m);

        // Act
        cart.AddItem(productId, additionalQuantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var productId = CartItemId.New();
        var invalidQuantity = 0;
        var price = new Money(10.0m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(productId, invalidQuantity, price));
    }
}
