using FluentAssertions;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Ordering.ValueObjects;

[Trait("Category", "Unit")]
public sealed class ShippingAddressTests
{
    [Fact]
    public void Constructor_ValidData_CreatesAddress()
    {
        // Arrange
        string name = "John Doe";
        string email = "john@example.com";
        string street = "123 Main St";
        string city = "San Francisco";
        string state = "CA";
        string zipCode = "94102";

        // Act
        ShippingAddress address = new(name, email, street, city, state, zipCode);

        // Assert
        address.Name.Should().Be(name);
        address.Email.Should().Be(email);
        address.Street.Should().Be(street);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.ZipCode.Should().Be(zipCode);
    }

    [Fact]
    public void Constructor_NullName_ThrowsArgumentException()
    {
        // Act
        Action act = () => new ShippingAddress(
            null!,
            "john@example.com",
            "123 Main St",
            "San Francisco",
            "CA",
            "94102");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EmptyEmail_ThrowsArgumentException()
    {
        // Act
        Action act = () => new ShippingAddress(
            "John Doe",
            string.Empty,
            "123 Main St",
            "San Francisco",
            "CA",
            "94102");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhitespaceStreet_ThrowsArgumentException()
    {
        // Act
        Action act = () => new ShippingAddress(
            "John Doe",
            "john@example.com",
            "   ",
            "San Francisco",
            "CA",
            "94102");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullCity_ThrowsArgumentException()
    {
        // Act
        Action act = () => new ShippingAddress(
            "John Doe",
            "john@example.com",
            "123 Main St",
            null!,
            "CA",
            "94102");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EmptyState_ThrowsArgumentException()
    {
        // Act
        Action act = () => new ShippingAddress(
            "John Doe",
            "john@example.com",
            "123 Main St",
            "San Francisco",
            string.Empty,
            "94102");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhitespaceZipCode_ThrowsArgumentException()
    {
        // Act
        Action act = () => new ShippingAddress(
            "John Doe",
            "john@example.com",
            "123 Main St",
            "San Francisco",
            "CA",
            "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
