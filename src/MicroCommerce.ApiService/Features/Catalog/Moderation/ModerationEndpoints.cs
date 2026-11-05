using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Moderation;

/// <summary>A Product as a Platform Operator reviews it: the Merchant's view plus who sells it.</summary>
public sealed record OperatorProductResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    string Status,
    IReadOnlyList<OptionResponse> Options,
    IReadOnlyList<VariantResponse> Variants,
    IReadOnlyList<ImageRef> Images,
    DateTimeOffset CreatedAt,
    int Version,
    Guid MerchantId,
    string? ShopName)
{
    public static async Task<OperatorProductResponse> FromAsync(Product p, IMerchantDirectory merchants, CancellationToken ct)
    {
        var m = MerchantProductResponse.From(p, imageBase: "/api/ops");
        var shops = await merchants.GetShopsAsync([p.MerchantId], ct);
        return new OperatorProductResponse(
            m.Id, m.Name, m.Description, m.CategoryId, m.Status, m.Options, m.Variants, m.Images, m.CreatedAt, m.Version,
            p.MerchantId.Value, shops.TryGetValue(p.MerchantId, out var shop) ? shop.ShopName : null);
    }
}

/// <summary>Any Product, whatever its status or Merchant, for review (FR-037).</summary>
public sealed class GetOperatorProductEndpoint(CatalogDbContext db, IMerchantDirectory merchants)
    : Endpoint<ProductIdRequest, OperatorProductResponse>
{
    public override void Configure()
    {
        Get("/products/{id}");
        Group<OpsGroup>();
        Summary(s =>
        {
            s.Summary = "Any Product, for review";
            s.Responses[404] = "No such Product.";
        });
    }

    public override async Task HandleAsync(ProductIdRequest req, CancellationToken ct)
    {
        var id = ProductId.From(req.Id);
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(await OperatorProductResponse.FromAsync(product, merchants, ct), ct);
    }
}

/// <summary>
/// A Platform Operator removes a Product: it leaves the Marketplace at once (FR-038). No version is
/// needed; a concurrent Merchant change is reloaded and the removal retried once, so moderation
/// always wins (research R5).
/// </summary>
public sealed class RemoveProductEndpoint(
    CatalogDbContext db,
    IMerchantDirectory merchants,
    ICurrentAccount currentAccount,
    ILogger<RemoveProductEndpoint> logger) : Endpoint<ProductIdRequest, OperatorProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id}/remove");
        Group<OpsGroup>();
        Summary(s =>
        {
            s.Summary = "Remove a Product from the Marketplace (final)";
            s.Responses[404] = "No such Product.";
        });
    }

    public override async Task HandleAsync(ProductIdRequest req, CancellationToken ct)
    {
        var id = ProductId.From(req.Id);
        for (var attempt = 1; ; attempt++)
        {
            var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, ct);
            if (product is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            product.Remove();
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt == 1)
            {
                db.ChangeTracker.Clear();
                continue;
            }

            currentAccount.TryGet(out var account);
            logger.LogInformation("Product {ProductId} removed by Platform Operator {AccountId}", product.Id, account?.Id);
            await Send.OkAsync(await OperatorProductResponse.FromAsync(product, merchants, ct), ct);
            return;
        }
    }
}
