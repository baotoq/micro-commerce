using Ardalis.GuardClauses;
using MicroCommerce.CartService.Application.Features.Carts;
using MicroCommerce.CartService.Domain.Carts;
using Shouldly;

namespace CartService.Application.UnitTests.Features.Carts;

public class GetCartQueryTests : TestBase
{
    [Fact]
    public async Task Handle_CartExist_ShouldReturnCart()
    {
        // Arrange
        var cart = Cart.Create();
        await SeedContext.Carts.AddAsync(cart);
        await SeedContext.SaveChangesAsync();

        var query = new GetCartQuery
        {
            CartId = cart.Id
        };

        // Act
        var sut = new GetCartQueryHandler(SeedContext);
        var act = await sut.Handle(query, CancellationToken.None);

        // Assert
        await Verify(act);
    }

    [Fact]
    public async Task Handle_CartDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var query = new GetCartQuery
        {
            CartId = CartId.New()
        };

        // Act
        var sut = new GetCartQueryHandler(SeedContext);

        // Assert
        await ThrowsTask(async () => await sut.Handle(query, CancellationToken.None));
    }
}
