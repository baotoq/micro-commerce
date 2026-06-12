using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Domain.Tests.Products;

public class ProductInventoryTests
{
    private static Product NewProduct(int inventory = 5) =>
        new(Sku.From("MC-TS-001"), "Test vase", "Vessels", 86m, inventory, ProductStatus.Active,
            photoUrls: ["https://example.com/test-vase.jpg"]);

    [Fact]
    public void DecrementInventory_ReducesCount()
    {
        var p = NewProduct(inventory: 5);
        p.DecrementInventory(2);
        Assert.Equal(3, p.Inventory);
    }

    [Fact]
    public void DecrementInventory_ToZero_IsAllowed()
    {
        var p = NewProduct(inventory: 2);
        p.DecrementInventory(2);
        Assert.Equal(0, p.Inventory);
    }

    [Fact]
    public void DecrementInventory_BelowZero_Throws()
    {
        var p = NewProduct(inventory: 1);
        Assert.Throws<InvalidOperationException>(() => p.DecrementInventory(2));
    }

    [Fact]
    public void DecrementInventory_NonPositiveQty_Throws()
    {
        var p = NewProduct();
        Assert.Throws<ArgumentOutOfRangeException>(() => p.DecrementInventory(0));
    }
}
