using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;

namespace MicroCommerce.CartService.Domain.UnitTests.Carts;

public class CartItemTests
{
    [Fact]
    public async Task Constructor_ShouldInitializeProperties_WhenValidArgumentsAreProvided()
    {
        // Arrange
        var productId = CartItemId.New();
        var quantity = 2;
        var priceAtPurchase = new Money(10.0m);

        // Act
        var cartItem = new CartItem(productId, quantity, priceAtPurchase);

        // Assert
        await Verify(cartItem);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var productId = CartItemId.New();
        var invalidQuantity = 0;
        var priceAtPurchase = new Money(10.0m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CartItem(productId, invalidQuantity, priceAtPurchase));
    }

    [Fact]
    public async Task IncreaseQuantity_ShouldIncreaseQuantity_WhenValidAmountIsProvided()
    {
        // Arrange
        var cartItem = new CartItem(CartItemId.New(), 1, new Money(10.0m));
        var amount = 2;

        // Act
        cartItem.IncreaseQuantity(amount);

        // Assert
        await Verify(cartItem);
    }

    [Fact]
    public void IncreaseQuantity_ShouldThrowException_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var cartItem = new CartItem(CartItemId.New(), 1, new Money(10.0m));
        var invalidAmount = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cartItem.IncreaseQuantity(invalidAmount));
    }
}
