using MicroCommerce.ProductService.Domain.DomainEvents;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.UnitTests;

public class ProductPriceUpdatedDomainEventTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var productId = new ProductId(Guid.NewGuid());
        var newPrice = 150;

        // Act
        var domainEvent = new ProductPriceUpdatedDomainEvent(productId, newPrice);

        // Assert
        Verify(domainEvent);
    }
}
