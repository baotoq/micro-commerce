using FluentValidation.TestHelper;
using MicroCommerce.ApiService.Features;
using MicroCommerce.ApiService.Features.Products;

namespace MicroCommerce.Tests.Features.Products;

public class CreateProductValidatorTests : TestBase
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
