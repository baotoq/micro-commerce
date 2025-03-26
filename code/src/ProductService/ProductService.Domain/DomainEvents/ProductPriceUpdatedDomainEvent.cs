using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.ProductService.Domain.Entities;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.DomainEvents;

public sealed record ProductPriceUpdatedDomainEvent(ProductId ProductId, Price NewPrice) : DomainEvent
{
    public ProductId ProductId { get; } = ProductId ?? throw new ArgumentNullException(nameof(ProductId));
    public Price NewPrice { get; } = NewPrice;
}
