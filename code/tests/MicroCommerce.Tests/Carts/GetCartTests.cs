using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.Carts;

namespace MicroCommerce.Tests.Carts;

public class GetCartTests : TestBase
{
    [Fact]
    public async Task Success()
    {
        // Arrange
        var cart = new Cart
        {
            Status = CartStatus.Pending
        };
        SeedContext.Carts.Add(cart);
        await SeedContext.SaveChangesAsync();

        var sut = new GetCart.Handler(SeedContext);

        // Act
        var act = await sut.Handle(new GetCart.Query { Id = cart.Id }, default);

        // Assert
        await Verify(act, VerifySettings);
    }

    [Fact]
    public async Task NotFound()
    {
        // Arrange
        var sut = new GetCart.Handler(SeedContext);

        // Act
        // Assert
        await ThrowsTask(() => sut.Handle(new GetCart.Query { Id = Guid.NewGuid() }, default), VerifySettings);
    }
}
