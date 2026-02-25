using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Integration.Builders;

/// <summary>
/// Fluent builder for creating Product entities in integration tests.
/// Provides sensible defaults that can be overridden for specific test scenarios.
/// </summary>
public sealed class ProductBuilder
{
    private string _name = "Test Product";
    private string _description = "Test Description";
    private decimal _price = 99.99m;
    private Guid _categoryId = Guid.NewGuid();
    private string? _imageUrl = null;
    private string? _sku = null;

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ProductBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public ProductBuilder WithSku(string? sku)
    {
        _sku = sku;
        return this;
    }

    public Product Build()
    {
        ProductName productName = ProductName.Create(_name);
        Money price = Money.Create(_price);
        CategoryId categoryId = CategoryId.From(_categoryId);

        return Product.Create(productName, _description, price, categoryId, _imageUrl, _sku);
    }
}
