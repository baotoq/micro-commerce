using MicroCommerce.ProductService.Domain.Entities;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.UnitTests;

public class ProductTests
{
    [Fact]
    public void UpdatePrice_ShouldUpdatePrice_WhenNewPriceIsValid()
    {
        // Arrange
        var productId = new ProductId(Guid.NewGuid());
        var product = new Product(productId, "Test Product", 100);

        // Act
        product.UpdatePrice(150);

        // Assert
        Verify(product);
    }

    [Fact]
    public void UpdatePrice_ShouldThrowException_WhenNewPriceIsInvalid()
    {
        // Arrange
        var productId = new ProductId(Guid.NewGuid());
        var product = new Product(productId, "Test Product", 100);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.UpdatePrice(0));
    }
}
