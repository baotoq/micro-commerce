using Ardalis.GuardClauses;

namespace MicroCommerce.InventoryService.Domain.Products;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string Sku { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Product(string name, string description, decimal price, int stockQuantity, string sku)
    {
        Id = Guid.NewGuid();
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Description = Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Price = Guard.Against.NegativeOrZero(price, nameof(price));
        StockQuantity = Guard.Against.Negative(stockQuantity, nameof(stockQuantity));
        Sku = Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int newQuantity)
    {
        StockQuantity = Guard.Against.Negative(newQuantity, nameof(newQuantity));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, decimal price)
    {
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Description = Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Price = Guard.Against.NegativeOrZero(price, nameof(price));
        UpdatedAt = DateTime.UtcNow;
    }
}
