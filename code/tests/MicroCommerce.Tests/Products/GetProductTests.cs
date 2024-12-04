using Ardalis.GuardClauses;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features;
using MicroCommerce.ApiService.Features.Products;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.Tests.Products;

public class GetProductTests : TestBase
{
    [Fact]
    public async Task GetProduct_Success()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product"
        };
        SeedContext.Products.Add(product);
        await SeedContext.SaveChangesAsync();

        var sut = new GetProduct.Handler(SeedContext);

        // Act
        var act = await sut.Handle(new GetProduct.Query { Id = product.Id }, default);

        // Assert
        await Verify(act, VerifySettings);
    }

    [Fact]
    public async Task GetProduct_NotFound()
    {
        // Arrange
        var sut = new GetProduct.Handler(SeedContext);

        // Act
        // Assert
        await ThrowsTask(() => sut.Handle(new GetProduct.Query { Id = Guid.NewGuid() }, default), VerifySettings);
    }
}
