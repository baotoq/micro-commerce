using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Domain.Tests.Products;

/// <summary>
/// Tests covering the rich-listing fields (Description, Tags, Weight, Origin, PhotoUrls)
/// added by the create-product wizard refactor.
/// </summary>
public class ProductRichFieldsTests
{
    private static Sku ValidSku() => Sku.From("MC-RT-001");

    private const string Name = "Persimmon vase";
    private const string Category = "Vessels";
    private const decimal Price = 86m;
    private const int Inventory = 24;
    private const string Origin = "Portland, OR";
    private const decimal Weight = 1.25m;
    private static readonly string[] OnePhoto = ["https://example.com/p1.jpg"];

    [Fact]
    public void Ctor_PopulatesRichFields()
    {
        var product = new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Active,
            description: "A tall hand-thrown vase.",
            tags: ["ceramic", "minimal", "handmade"],
            weight: Weight,
            origin: Origin,
            photoUrls: OnePhoto);

        Assert.Equal("A tall hand-thrown vase.", product.Description);
        Assert.Equal(new[] { "ceramic", "minimal", "handmade" }, product.Tags);
        Assert.Equal(Weight, product.Weight);
        Assert.Equal(Origin, product.Origin);
        Assert.Equal(OnePhoto, product.PhotoUrls);
    }

    [Fact]
    public void Ctor_DescriptionIsNullable_AcceptsNull()
    {
        var product = new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: [],
            weight: Weight,
            origin: Origin,
            photoUrls: []);

        Assert.Null(product.Description);
    }

    [Fact]
    public void Ctor_DeduplicatesTags()
    {
        var product = new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: ["ceramic", "ceramic", "minimal"],
            weight: Weight,
            origin: Origin,
            photoUrls: []);

        Assert.Equal(new[] { "ceramic", "minimal" }, product.Tags);
    }

    [Fact]
    public void Ctor_PreservesTagOrderAfterDedup()
    {
        var product = new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: ["b", "a", "b", "c", "a"],
            weight: Weight,
            origin: Origin,
            photoUrls: []);

        Assert.Equal(new[] { "b", "a", "c" }, product.Tags);
    }

    [Fact]
    public void Ctor_ZeroWeight_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: [],
            weight: 0m,
            origin: Origin,
            photoUrls: []));
    }

    [Fact]
    public void Ctor_NegativeWeight_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: [],
            weight: -0.01m,
            origin: Origin,
            photoUrls: []));
    }

    [Fact]
    public void Ctor_EmptyOrigin_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: [],
            weight: Weight,
            origin: "",
            photoUrls: []));
    }

    [Fact]
    public void Ctor_ActiveStatusWithNoPhotos_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Active,
            description: null,
            tags: [],
            weight: Weight,
            origin: Origin,
            photoUrls: []));
    }

    [Fact]
    public void Ctor_DraftStatusWithNoPhotos_Allowed()
    {
        var product = new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: [],
            weight: Weight,
            origin: Origin,
            photoUrls: []);

        Assert.Empty(product.PhotoUrls);
    }

    [Fact]
    public void Update_ChangesRichFields()
    {
        var product = new Product(
            ValidSku(), Name, Category, Price, Inventory, ProductStatus.Draft,
            description: null,
            tags: [],
            weight: Weight,
            origin: Origin,
            photoUrls: []);

        product.Update(
            "Updated", "Tableware", 99m, 5, ProductStatus.Active,
            description: "Updated body.",
            tags: ["new"],
            weight: 2m,
            origin: "Seattle, WA",
            photoUrls: ["https://example.com/p2.jpg"]);

        Assert.Equal("Updated body.", product.Description);
        Assert.Equal(new[] { "new" }, product.Tags);
        Assert.Equal(2m, product.Weight);
        Assert.Equal("Seattle, WA", product.Origin);
        Assert.Equal(new[] { "https://example.com/p2.jpg" }, product.PhotoUrls);
    }
}
