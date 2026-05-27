using MicroCommerce.Catalog.Api.SeedData;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.SeedData;

public class ProductSeederTests
{
    [Fact]
    public async Task SeedAsync_OnEmptyDb_AddsAllSeedRecords()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;
        var contentRoot = WriteSeedFile("""
        [
          { "sku": "MC-VS-001", "name": "Persimmon vase", "category": "Vessels", "price": 86, "inventory": 24, "status": "active", "views7d": 412 },
          { "sku": "MC-VS-002", "name": "Linen vase",     "category": "Vessels", "price": 68, "inventory": 19, "status": "active", "views7d": 188 }
        ]
        """);

        await ProductSeeder.SeedAsync(db, contentRoot, ct);

        var products = await db.Products.ToListAsync(ct);
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p.Sku.Value == "MC-VS-001");
        Assert.Contains(products, p => p.Sku.Value == "MC-VS-002");
    }

    [Fact]
    public async Task SeedAsync_WithSomeRecordsAlreadyPresent_AddsOnlyMissingOnes()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;

        db.Products.Add(new Product(
            Sku.From("MC-VS-001"), "Existing user copy", "Vessels", 50m, 5, ProductStatus.Draft, views7d: 100));
        await db.SaveChangesAsync(ct);

        var contentRoot = WriteSeedFile("""
        [
          { "sku": "MC-VS-001", "name": "Persimmon vase", "category": "Vessels", "price": 86, "inventory": 24, "status": "active", "views7d": 412 },
          { "sku": "MC-VS-002", "name": "Linen vase",     "category": "Vessels", "price": 68, "inventory": 19, "status": "active", "views7d": 188 }
        ]
        """);

        await ProductSeeder.SeedAsync(db, contentRoot, ct);

        var products = await db.Products.ToListAsync(ct);
        Assert.Equal(2, products.Count);

        var existing = products.Single(p => p.Sku.Value == "MC-VS-001");
        Assert.Equal("Existing user copy", existing.Name);
        Assert.Equal(50m, existing.Price);

        Assert.Contains(products, p => p.Sku.Value == "MC-VS-002");
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateRecords()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;
        var contentRoot = WriteSeedFile("""
        [
          { "sku": "MC-VS-001", "name": "Persimmon vase", "category": "Vessels", "price": 86, "inventory": 24, "status": "active", "views7d": 412 }
        ]
        """);

        await ProductSeeder.SeedAsync(db, contentRoot, ct);
        await ProductSeeder.SeedAsync(db, contentRoot, ct);

        Assert.Equal(1, await db.Products.CountAsync(ct));
    }

    [Fact]
    public async Task SeedAsync_WithMissingFile_IsNoOp()
    {
        await using var db = DbContextFactory.Create();
        var ct = TestContext.Current.CancellationToken;
        var contentRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        await ProductSeeder.SeedAsync(db, contentRoot, ct);

        Assert.Equal(0, await db.Products.CountAsync(ct));
    }

    private static string WriteSeedFile(string json)
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"seedtest-{Guid.NewGuid()}");
        var dir = Path.Combine(contentRoot, "SeedData");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "products.json"), json);
        return contentRoot;
    }
}
