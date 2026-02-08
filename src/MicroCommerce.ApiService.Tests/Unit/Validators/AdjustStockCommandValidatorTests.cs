using FluentAssertions;
using FluentValidation.TestHelper;
using MicroCommerce.ApiService.Features.Inventory.Application.Commands.AdjustStock;

namespace MicroCommerce.ApiService.Tests.Unit.Validators;

[Trait("Category", "Unit")]
public sealed class AdjustStockCommandValidatorTests
{
    private readonly AdjustStockCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdjustStockCommand command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyProductId_ShouldHaveError()
    {
        // Arrange
        AdjustStockCommand command = CreateValidCommand() with { ProductId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Validate_ZeroAdjustment_ShouldHaveError()
    {
        // Arrange
        AdjustStockCommand command = CreateValidCommand() with { Adjustment = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Adjustment);
    }

    [Fact]
    public void Validate_PositiveAdjustment_ShouldNotHaveError()
    {
        // Arrange
        AdjustStockCommand command = CreateValidCommand() with { Adjustment = 10 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NegativeAdjustment_ShouldNotHaveError()
    {
        // Arrange
        AdjustStockCommand command = CreateValidCommand() with { Adjustment = -5 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Helper methods

    private static AdjustStockCommand CreateValidCommand()
    {
        return new AdjustStockCommand(
            Guid.NewGuid(),
            10,
            "Restock",
            "admin@test.com");
    }
}
