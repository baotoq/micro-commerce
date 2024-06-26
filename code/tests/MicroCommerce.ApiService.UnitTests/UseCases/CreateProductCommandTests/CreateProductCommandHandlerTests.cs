using MicroCommerce.ApiService.UseCases.Products;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.ApiService.UnitTests.UseCases.CreateProductCommandTests;

public class CreateProductCommandHandlerTests : TestBase
{
    [Fact]
    public async Task CreateProduct()
    {
        // Arrange
        var request = new CreateProductCommand
        {
            Name = "Apple",
            Price = 1,
            RemainingStock = 2
        };
        var handler = new CreateProductCommandHandler(Context, NullLogger<CreateProductCommandHandler>.Instance, null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
    }
}