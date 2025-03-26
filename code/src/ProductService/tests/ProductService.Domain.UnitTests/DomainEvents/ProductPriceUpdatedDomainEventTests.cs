using MicroCommerce.ProductService.Domain.DomainEvents;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.UnitTests.DomainEvents;

public class ProductPriceUpdatedDomainEventTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var productId = new ProductId(Guid.NewGuid());
        var newPrice = 150;

        // Act
        var domainEvent = new ProductPriceUpdatedDomainEvent(productId, new Price(newPrice));

        // Assert
        Verify(domainEvent);
    }
}
