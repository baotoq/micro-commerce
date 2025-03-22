using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.ProductService.Domain.Entities;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.DomainEvents;

public sealed class ProductPriceUpdatedDomainEvent(ProductId productId, decimal newPrice) : DomainEvent
{
    public ProductId ProductId { get; } = productId ?? throw new ArgumentNullException(nameof(productId));
    public decimal NewPrice { get; } = newPrice;
}
