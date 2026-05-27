using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Domain.Tests.Products;

public class ProductTests
{
    private static Sku ValidSku() => Sku.From("MC-001");

    private static Product Create(
        string name = "Widget",
        string category = "Electronics",
        decimal price = 9.99m,
        int inventory = 10,
        ProductStatus status = ProductStatus.Draft) =>
        new(ValidSku(), name, category, price, inventory, status);

    [Fact]
    public void Product_ValidArgs_CreatesWithNewId()
    {
        var product = Create();
        Assert.NotEqual(default, product.Id.Value);
    }

    [Fact]
    public void Product_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create(name: ""));
    }

    [Fact]
    public void Product_EmptyCategory_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create(category: ""));
    }

    [Fact]
    public void Product_NegativePrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(price: -1m));
    }

    [Fact]
    public void Product_NegativeInventory_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(inventory: -1));
    }

    [Fact]
    public void Product_Update_ChangesProperties()
    {
        var product = Create();
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
        var product = Create();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            product.Update("Widget", "Electronics", -1m, 10, ProductStatus.Draft));
    }

    [Fact]
    public void Product_Update_EmptyName_Throws()
    {
        var product = Create();
        Assert.Throws<ArgumentException>(() =>
            product.Update("", "Electronics", 9.99m, 10, ProductStatus.Draft));
    }

    [Fact]
    public void Product_Update_EmptyCategory_Throws()
    {
        var product = Create();
        Assert.Throws<ArgumentException>(() =>
            product.Update("Widget", "", 9.99m, 10, ProductStatus.Draft));
    }

    [Fact]
    public void Product_Update_NegativeInventory_Throws()
    {
        var product = Create();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            product.Update("Widget", "Electronics", 9.99m, -1, ProductStatus.Draft));
    }
}
