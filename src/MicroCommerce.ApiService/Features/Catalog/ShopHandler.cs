using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// Keeps the Catalog's <see cref="Shop"/>s in step with the Accounts module's Merchants. Events arrive at least once
/// and in any order, so each is applied only if it is newer than the Shop's current <see cref="Shop.ProfileVersion"/>.
/// </summary>
public static class ShopHandler
{
    public static Task Handle(MerchantOpened e, CatalogDbContext db, StorefrontProductCache cache, CancellationToken ct) =>
        ApplyAsync(db, cache, e.MerchantId, e.ShopName, e.Description, e.ProfileVersion, ct);

    public static Task Handle(MerchantProfileChanged e, CatalogDbContext db, StorefrontProductCache cache, CancellationToken ct) =>
        ApplyAsync(db, cache, e.MerchantId, e.ShopName, e.Description, e.ProfileVersion, ct);

    private static async Task ApplyAsync(
        CatalogDbContext db,
        StorefrontProductCache cache,
        MerchantId merchantId,
        string shopName,
        string? description,
        int profileVersion,
        CancellationToken ct)
    {
        // One statement, so concurrent deliveries for the same Merchant can't both insert or overwrite a newer version.
        var applied = await db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO catalog."Shops" ("Id", "ShopName", "Description", "ProfileVersion")
            VALUES ({merchantId.Value}, {shopName}, {description}, {profileVersion})
            ON CONFLICT ("Id") DO UPDATE
            SET "ShopName" = EXCLUDED."ShopName", "Description" = EXCLUDED."Description", "ProfileVersion" = EXCLUDED."ProfileVersion"
            WHERE "Shops"."ProfileVersion" < EXCLUDED."ProfileVersion"
            """,
            ct);
        if (applied == 0) return;

        // Cached Product details show the Shop name.
        var productIds = await db.Products.Where(p => p.MerchantId == merchantId).Select(p => p.Id).ToListAsync(ct);
        await cache.EvictAsync(productIds, ct);
    }
}
