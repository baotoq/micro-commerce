using FluentAssertions;
using FluentValidation.TestHelper;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;

namespace MicroCommerce.ApiService.Tests.Unit.Validators;

[Trait("Category", "Unit")]
public sealed class SubmitOrderCommandValidatorTests
{
    private readonly SubmitOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        SubmitOrderCommand command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveErrorForEmail()
    {
        // Arrange
        SubmitOrderCommand command = CreateValidCommand() with { Email = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidEmail_ShouldHaveErrorForEmail()
    {
        // Arrange
        SubmitOrderCommand command = CreateValidCommand() with { Email = "not-an-email" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_NullShippingAddress_ShouldHaveError()
    {
        // Arrange
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress);
    }

    [Fact]
    public void Validate_EmptyShippingName_ShouldHaveError()
    {
        // Arrange
        var address = CreateValidAddress() with { Name = "" };
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = address };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.Name);
    }

    [Fact]
    public void Validate_EmptyShippingStreet_ShouldHaveError()
    {
        // Arrange
        var address = CreateValidAddress() with { Street = "" };
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = address };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.Street);
    }

    [Fact]
    public void Validate_EmptyShippingCity_ShouldHaveError()
    {
        // Arrange
        var address = CreateValidAddress() with { City = "" };
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = address };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.City);
    }

    [Fact]
    public void Validate_EmptyShippingState_ShouldHaveError()
    {
        // Arrange
        var address = CreateValidAddress() with { State = "" };
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = address };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.State);
    }

    [Fact]
    public void Validate_EmptyShippingZipCode_ShouldHaveError()
    {
        // Arrange
        var address = CreateValidAddress() with { ZipCode = "" };
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = address };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.ZipCode);
    }

    [Fact]
    public void Validate_ZipCodeTooLong_ShouldHaveError()
    {
        // Arrange
        var address = CreateValidAddress() with { ZipCode = "12345678901" }; // 11 chars
        SubmitOrderCommand command = CreateValidCommand() with { ShippingAddress = address };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.ZipCode);
    }

    [Fact]
    public void Validate_EmptyItems_ShouldHaveError()
    {
        // Arrange
        SubmitOrderCommand command = CreateValidCommand() with { Items = [] };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Validate_ItemWithZeroQuantity_ShouldHaveError()
    {
        // Arrange
        var item = CreateValidOrderItem() with { Quantity = 0 };
        SubmitOrderCommand command = CreateValidCommand() with { Items = [item] };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Validate_ItemWithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var item = CreateValidOrderItem() with { UnitPrice = -10.00m };
        SubmitOrderCommand command = CreateValidCommand() with { Items = [item] };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }

    [Fact]
    public void Validate_ItemWithEmptyProductId_ShouldHaveError()
    {
        // Arrange
        var item = CreateValidOrderItem() with { ProductId = Guid.Empty };
        SubmitOrderCommand command = CreateValidCommand() with { Items = [item] };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    // Helper methods

    private static SubmitOrderCommand CreateValidCommand()
    {
        return new SubmitOrderCommand(
            Guid.NewGuid(),
            "buyer@test.com",
            CreateValidAddress(),
            [CreateValidOrderItem()]);
    }

    private static ShippingAddressRequest CreateValidAddress()
    {
        return new ShippingAddressRequest(
            "John Doe",
            "john@example.com",
            "123 Main St",
            "San Francisco",
            "CA",
            "94102");
    }

    private static OrderItemRequest CreateValidOrderItem()
    {
        return new OrderItemRequest(
            Guid.NewGuid(),
            "Test Product",
            99.99m,
            null,
            1);
    }
}
