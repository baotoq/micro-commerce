using MicroCommerce.ApiService.Features;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.Tests;

public class CreateProductCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Success()
    {
        var sut = new CreateProductCommandHandler(Context, NullLogger<CreateProductCommandHandler>.Instance, null);

        var act = await sut.Handle(new CreateProductCommand
        {
            Name = "Product 1",
            Price = 100,
            RemainingStock = 101
        }, default);

        await Verify(act);
    }
}
