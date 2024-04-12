using Api.UseCases.Products;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.UnitTests.CreateProductCommandTests;

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
        var handler = new CreateProductCommandHandler(Context, NullLogger<CreateProductCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
    }
}