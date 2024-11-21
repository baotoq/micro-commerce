using MicroCommerce.ApiService.Features;
using MicroCommerce.ApiService.Features.Products;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.Tests.Products;

public class CreateProductTests : TestBase
{
    [Fact]
    public async Task CreateProduct_Success()
    {
        var sut = new CreateProduct.Handler(Context, NullLogger<CreateProduct.Handler>.Instance);

        var act = await sut.Handle(new CreateProduct.Command
        {
            Name = "Product 1",
            Price = 100,
            RemainingStock = 101
        }, default);

        await Verify(act, VerifySettings);
    }
}
