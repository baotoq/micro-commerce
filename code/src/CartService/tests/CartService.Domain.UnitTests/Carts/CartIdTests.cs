using MicroCommerce.CartService.Domain.Carts;

namespace MicroCommerce.CartService.Domain.UnitTests.Carts;

public class CartIdTests
{
    [Fact]
    public async Task New_ShouldGenerateNonEmptyCartId()
    {
        // Act
        var cartId = CartId.New();

        // Assert
        await Verify(cartId);
    }

    [Fact]
    public async Task From_ShouldCreateCartId_WhenValidGuidIsProvided()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var cartId = CartId.From(guid);

        // Assert
        await Verify(cartId);
    }
}
