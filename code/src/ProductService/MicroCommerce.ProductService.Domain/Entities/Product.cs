using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.Entities;

public class Product : BaseAggregateRoot<ProductId>
{
    public string Name { get; private set; }
    public Price Price { get; private set; }
    public int Stock { get; private set; }

    public Product(ProductId id, string name, Price price, int stock) : base(id)
    {
        Name = name;
        Price = price;
        Stock = stock;
    }

    public void UpdatePrice(Price newPrice)
    {
        Price = newPrice;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        Stock += quantity;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (quantity > Stock)
            throw new InvalidOperationException("Insufficient stock to remove the specified quantity.");

        Stock -= quantity;
    }
}
