using FluentValidation.TestHelper;
using MicroCommerce.ApiService.Features;

namespace MicroCommerce.Tests;

public class CreateProductValidatorTests
{
    private readonly CreateProduct.Validator _validator = new();

    [Fact]
    public async Task AllError()
    {
        // Arrange
        var request = new CreateProduct.Command
        {
            Name = "",
            Price = 0,
            RemainingStock = 0
        };

        // Act
        var act = await _validator.TestValidateAsync(request);

        // Assert
        await Verify(act);
    }
}
