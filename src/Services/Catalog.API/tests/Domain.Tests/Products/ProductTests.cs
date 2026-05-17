using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Domain.Tests.Products;

public class ProductTests
{
    private static Sku ValidSku() => Sku.From("MC-001");

    [Fact]
    public void Product_ValidArgs_CreatesWithNewId()
    {
        var product = new Product(ValidSku(), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active);
        Assert.NotEqual(default, product.Id.Value);
    }

    [Fact]
    public void Product_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(ValidSku(), "", "Electronics", 9.99m, 10, ProductStatus.Active));
    }

    [Fact]
    public void Product_EmptyCategory_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(ValidSku(), "Widget", "", 9.99m, 10, ProductStatus.Active));
    }

    [Fact]
    public void Product_NegativePrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product(ValidSku(), "Widget", "Electronics", -1m, 10, ProductStatus.Active));
    }

    [Fact]
    public void Product_NegativeInventory_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product(ValidSku(), "Widget", "Electronics", 9.99m, -1, ProductStatus.Active));
    }

    [Fact]
    public void Product_Update_ChangesProperties()
    {
        var product = new Product(ValidSku(), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active);
        product.Update("Updated Widget", "Gadgets", 19.99m, 5, ProductStatus.Low);

        Assert.Equal("Updated Widget", product.Name);
        Assert.Equal("Gadgets", product.Category);
        Assert.Equal(19.99m, product.Price);
        Assert.Equal(5, product.Inventory);
        Assert.Equal(ProductStatus.Low, product.Status);
    }

    [Fact]
    public void Product_Update_NegativePrice_Throws()
    {
        var product = new Product(ValidSku(), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            product.Update("Widget", "Electronics", -1m, 10, ProductStatus.Active));
    }
}
