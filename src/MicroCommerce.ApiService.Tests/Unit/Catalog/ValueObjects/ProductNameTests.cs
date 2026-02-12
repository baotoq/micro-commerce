using FluentAssertions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Catalog.ValueObjects;

[Trait("Category", "Unit")]
public sealed class ProductNameTests
{
    [Fact]
    public void Create_ValidName_ReturnsProductName()
    {
        // Arrange
        string name = "Test Product";

        // Act
        ProductName productName = ProductName.Create(name);

        // Assert
        productName.Value.Should().Be(name);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        // Arrange
        string nameWithWhitespace = "  Test Product  ";

        // Act
        ProductName productName = ProductName.Create(nameWithWhitespace);

        // Assert
        productName.Value.Should().Be("Test Product");
    }

    [Fact]
    public void Create_MinLength_Succeeds()
    {
        // Arrange
        string twoCharName = "AB";

        // Act
        ProductName productName = ProductName.Create(twoCharName);

        // Assert
        productName.Value.Should().Be(twoCharName);
    }

    [Fact]
    public void Create_MaxLength_Succeeds()
    {
        // Arrange
        string maxLengthName = new string('A', 200);

        // Act
        ProductName productName = ProductName.Create(maxLengthName);

        // Assert
        productName.Value.Should().Be(maxLengthName);
        productName.Value.Length.Should().Be(200);
    }

    [Fact]
    public void Create_TooShort_ThrowsException()
    {
        // Arrange
        string oneCharName = "A";

        // Act
        Action act = () => ProductName.Create(oneCharName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Create_TooLong_ThrowsException()
    {
        // Arrange
        string tooLongName = new string('A', 201);

        // Act
        Action act = () => ProductName.Create(tooLongName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Create_NullOrWhitespace_ThrowsException()
    {
        // Arrange & Act & Assert
        Action actNull = () => ProductName.Create(null!);
        Action actEmpty = () => ProductName.Create("");
        Action actWhitespace = () => ProductName.Create("   ");

        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
        actWhitespace.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ImplicitConversion_ReturnsStringValue()
    {
        // Arrange
        ProductName productName = ProductName.Create("Test Product");

        // Act
        string stringValue = productName;

        // Assert
        stringValue.Should().Be("Test Product");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        ProductName name1 = ProductName.Create("Test Product");
        ProductName name2 = ProductName.Create("Test Product");

        // Act & Assert
        name1.Should().Be(name2);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        // Arrange
        ProductName name1 = ProductName.Create("Product A");
        ProductName name2 = ProductName.Create("Product B");

        // Act & Assert
        name1.Should().NotBe(name2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        ProductName productName = ProductName.Create("Test Product");

        // Act
        string result = productName.ToString();

        // Assert
        result.Should().Be("Test Product");
    }
}
