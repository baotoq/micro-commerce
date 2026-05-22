using System.Text.Json;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class ProductSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Idempotent: inserts any seed records whose SKU is not already in the DB.
    // Existing rows are left untouched so post-seed mutations (e.g. e2e deletes)
    // self-heal on the next AppHost start.
    public static async Task SeedAsync(AppDbContext db, string contentRoot, CancellationToken ct = default)
    {
        var path = Path.Combine(contentRoot, "SeedData", "products.json");
        if (!File.Exists(path)) return;

        await using var stream = File.OpenRead(path);
        var records = await JsonSerializer.DeserializeAsync<List<SeedRecord>>(stream, JsonOptions, ct)
            ?? throw new InvalidOperationException($"Failed to deserialize seed data at {path}.");

        var existingSkus = (await db.Products.ToListAsync(ct))
            .Select(p => p.Sku.Value)
            .ToHashSet();

        var added = false;
        foreach (var r in records)
        {
            if (existingSkus.Contains(r.Sku)) continue;

            db.Products.Add(new Product(
                Sku.From(r.Sku),
                r.Name,
                r.Category,
                r.Price,
                r.Inventory,
                ParseStatus(r.Status),
                r.Views7d));
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }

    private static ProductStatus ParseStatus(string s) => s.ToLowerInvariant() switch
    {
        "active" => ProductStatus.Active,
        "low" => ProductStatus.Low,
        "out" => ProductStatus.Out,
        "draft" => ProductStatus.Draft,
        _ => throw new ArgumentException($"Invalid seed status: {s}"),
    };

    private sealed record SeedRecord(
        string Sku,
        string Name,
        string Category,
        decimal Price,
        int Inventory,
        string Status,
        int Views7d);
}
