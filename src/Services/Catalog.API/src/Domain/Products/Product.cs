namespace MicroCommerce.Catalog.Domain.Products;

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
    public string? Description { get; private set; }
    public IReadOnlyList<string> Tags { get; private set; } = [];
    public decimal Weight { get; private set; }
    public string Origin { get; private set; } = string.Empty;
    public IReadOnlyList<string> PhotoUrls { get; private set; } = [];

    private Product() { }

    public Product(
        Sku sku,
        string name,
        string category,
        decimal price,
        int inventory,
        ProductStatus status,
        string? description = null,
        IEnumerable<string>? tags = null,
        decimal weight = 0.01m,
        string origin = "Unknown, NA",
        IEnumerable<string>? photoUrls = null,
        int views7d = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category required.", nameof(category));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (inventory < 0) throw new ArgumentOutOfRangeException(nameof(inventory), "Inventory cannot be negative.");
        if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        if (string.IsNullOrWhiteSpace(origin)) throw new ArgumentException("Origin required.", nameof(origin));

        var dedupedTags = DedupePreserveOrder(tags);
        var photos = photoUrls?.ToArray() ?? [];

        if (status == ProductStatus.Active && photos.Length == 0)
            throw new ArgumentException("Active products require at least one photo.", nameof(photoUrls));

        Id = ProductId.New();
        Sku = sku;
        Name = name;
        Category = category;
        Price = price;
        Inventory = inventory;
        Status = status;
        Views7d = views7d;
        Description = description;
        Tags = dedupedTags;
        Weight = weight;
        Origin = origin;
        PhotoUrls = photos;
    }

    public void Update(
        string name,
        string category,
        decimal price,
        int inventory,
        ProductStatus status,
        string? description = null,
        IEnumerable<string>? tags = null,
        decimal weight = 0.01m,
        string origin = "Unknown, NA",
        IEnumerable<string>? photoUrls = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category required.", nameof(category));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (inventory < 0) throw new ArgumentOutOfRangeException(nameof(inventory));
        if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        if (string.IsNullOrWhiteSpace(origin)) throw new ArgumentException("Origin required.", nameof(origin));

        var dedupedTags = DedupePreserveOrder(tags);
        var photos = photoUrls?.ToArray() ?? [];

        if (status == ProductStatus.Active && photos.Length == 0)
            throw new ArgumentException("Active products require at least one photo.", nameof(photoUrls));

        Name = name;
        Category = category;
        Price = price;
        Inventory = inventory;
        Status = status;
        Description = description;
        Tags = dedupedTags;
        Weight = weight;
        Origin = origin;
        PhotoUrls = photos;
    }

    private static string[] DedupePreserveOrder(IEnumerable<string>? tags)
    {
        if (tags is null) return [];
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<string>();
        foreach (var t in tags)
        {
            if (seen.Add(t)) ordered.Add(t);
        }
        return ordered.ToArray();
    }
}
