using Api.UseCases.Products;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.UnitTests;

public class CreateProductCommandTests : TestBase
{
    [Fact]
    public async Task CreateProduct()
    {
        // Arrange
        var request = new CreateProductCommand("Apple");
        var handler = new CreateProductCommandHandler(Context, NullLogger<CreateProductCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
    }
}