using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.UnitTests.ValueObjects;

public class PriceTests
{
    [Fact]
    public async Task Constructor_ShouldInitializeProperties_WhenValidArgumentsProvided()
    {
        // Arrange
        var amount = 100m;

        // Act
        var price = new Price(amount);

        // Assert
        await Verify(price);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenAmountIsZeroOrNegative()
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Price(0));
        Assert.Throws<ArgumentException>(() => new Price(-10));
    }

    [Fact]
    public async Task ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var price = new Price(100m);

        // Act
        var result = price.ToString();

        // Assert
        await Verify(result);
    }
}
