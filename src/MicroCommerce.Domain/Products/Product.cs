namespace MicroCommerce.Domain.Products;

public class Product
{
    public ProductId Id { get; private set; }
    public Sku Sku { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Inventory { get; private set; }
    public ProductStatus Status { get; private set; }
    public int Views7d { get; private set; }

    private Product() { }

    public Product(Sku sku, string name, string category, decimal price, int inventory, ProductStatus status, int views7d = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category required.", nameof(category));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (inventory < 0) throw new ArgumentOutOfRangeException(nameof(inventory), "Inventory cannot be negative.");

        Id = ProductId.New();
        Sku = sku;
        Name = name;
        Category = category;
        Price = price;
        Inventory = inventory;
        Status = status;
        Views7d = views7d;
    }

    public void Update(string name, string category, decimal price, int inventory, ProductStatus status)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category required.", nameof(category));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (inventory < 0) throw new ArgumentOutOfRangeException(nameof(inventory));

        Name = name;
        Category = category;
        Price = price;
        Inventory = inventory;
        Status = status;
    }
}
