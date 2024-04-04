using Api.UseCases.Products;

namespace Api.UnitTests.CreateProductCommandTests;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public async Task AllError()
    {
        // Arrange
        var request = new CreateProductCommand();
        
        // Act
        var result = await _validator.TestValidateAsync(request);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(p => p.Name);
        result.ShouldHaveValidationErrorFor(p => p.Price);
        result.ShouldHaveValidationErrorFor(p => p.RemainingStock);
    }
}