using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.Entities;

public class Product(ProductId id, string name, decimal price) : BaseAggregateRoot<ProductId>(id)
{
    public string Name { get; private set; } = name;
    public decimal Price { get; private set; } = price;

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero.");

        Price = newPrice;
    }
}
