using MicroCommerce.ProductService.Domain.Entities;
using MicroCommerce.ProductService.Domain.ValueObjects;
using MicroCommerce.ProductService.Domain.UnitTests.Builders;

namespace MicroCommerce.ProductService.Domain.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void UpdatePrice_ShouldUpdatePrice_WhenNewPriceIsValid()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(new Price(100))
            .Build();

        // Act
        product.UpdatePrice(new Price(150));

        // Assert
        Verify(product);
    }

    [Fact]
    public void UpdatePrice_ShouldThrowException_WhenNewPriceIsInvalid()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(new Price(150))
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.UpdatePrice(new Price(-150)));
    }

    [Fact]
    public async Task AddStock_ShouldIncreaseStock()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(5)
            .Build();

        // Act
        product.AddStock(10);

        // Assert
        await Verify(product);
    }

    [Fact]
    public async Task RemoveStock_ShouldDecreaseStock()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(15)
            .Build();

        // Act
        product.RemoveStock(5);

        // Assert
        await Verify(product);
    }

    [Fact]
    public void RemoveStock_ShouldThrowException_WhenStockIsInsufficient()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(5)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => product.RemoveStock(10));
    }

    [Fact]
    public void AddStock_ShouldThrowException_WhenQuantityIsInvalid()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(5)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.AddStock(0));
    }

    [Fact]
    public void RemoveStock_ShouldThrowException_WhenQuantityIsInvalid()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(5)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.RemoveStock(0));
    }
}
