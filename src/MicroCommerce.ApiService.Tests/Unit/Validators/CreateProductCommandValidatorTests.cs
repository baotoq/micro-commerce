using FluentAssertions;
using FluentValidation.TestHelper;
using MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateProduct;

namespace MicroCommerce.ApiService.Tests.Unit.Validators;

[Trait("Category", "Unit")]
public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { Name = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooShort_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { Name = "A" }; // 1 char

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { Name = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyDescription_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { Description = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NegativePrice_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { Price = -10.00m };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_EmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { CategoryId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_InvalidImageUrl_ShouldHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { ImageUrl = "not-a-url" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ValidImageUrl_ShouldNotHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { ImageUrl = "https://example.com/image.jpg" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_NullImageUrl_ShouldNotHaveError()
    {
        // Arrange
        CreateProductCommand command = CreateValidCommand() with { ImageUrl = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    // Helper methods

    private static CreateProductCommand CreateValidCommand()
    {
        return new CreateProductCommand(
            "Test Product",
            "Test Description",
            99.99m,
            Guid.NewGuid(),
            "https://example.com/image.jpg",
            "SKU-123");
    }
}
