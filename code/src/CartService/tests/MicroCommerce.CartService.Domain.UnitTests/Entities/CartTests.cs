using MicroCommerce.CartService.Domain.Common;
using MicroCommerce.CartService.Domain.UnitTests.Builders;

namespace MicroCommerce.CartService.Domain.UnitTests.Entities;

public class CartTests
{
    [Fact]
    public async Task AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var productId = Guid.NewGuid();
        var quantity = 2;
        var price = new Price(10.0m);

        // Act
        cart.AddItem(productId, quantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public async Task AddItem_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var cart = new CartBuilder().WithItem(Guid.NewGuid(), 1, new Price(10.0m)).Build();
        var productId = Guid.NewGuid();
        var additionalQuantity = 2;
        var price = new Price(10.0m);

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
        var productId = Guid.NewGuid();
        var invalidQuantity = 0;
        var price = new Price(10.0m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(productId, invalidQuantity, price));
    }
}
