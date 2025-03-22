using MicroCommerce.ProductService.Domain.Entities;
using MicroCommerce.ProductService.Domain.ValueObjects;

namespace MicroCommerce.ProductService.Domain.UnitTests.Builders;

public class ProductBuilder
{
    private ProductId _id = new(Guid.NewGuid());
    private string _name = "Default Product";
    private Price _price = new(100m);
    private int _stock = 10;

    public ProductBuilder WithId(ProductId id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithPrice(Price price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public Product Build()
    {
        return new Product(_id, _name, _price, _stock);
    }
}
