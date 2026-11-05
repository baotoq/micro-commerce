using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Catalog.Marketplace;
using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;
using MicroCommerce.ApiService.Features.Catalog.Moderation;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace MicroCommerce.ApiService.Features.Catalog.ProductImages;

public sealed record ImageRequest(Guid ProductId, Guid ImageId);

/// <summary>
/// Streams one Product image. An image ID's content never changes, so the ETag is the ID, and
/// browsers revalidate (<c>no-cache</c>): a hidden Product's images stop being served at once
/// (FR-025, research R7).
/// </summary>
internal static class ImageStreaming
{
    public static async Task SendAsync(HttpContext http, IProductImageStore store, Product? product, Guid imageId, bool isPrivate, CancellationToken ct)
    {
        var id = ProductImageId.From(imageId);
        if (product is null || !product.HasImage(id))
        {
            await http.Response.SendNotFoundAsync(ct);
            return;
        }

        var etag = new EntityTagHeaderValue($"\"{imageId:N}\"");
        http.Response.Headers.CacheControl = isPrivate ? "private, no-cache" : "no-cache";
        http.Response.Headers.ETag = etag.ToString();
        http.Response.Headers.XContentTypeOptions = "nosniff";

        if (http.Request.GetTypedHeaders().IfNoneMatch.Any(tag => tag.Compare(etag, useStrongComparison: true)))
        {
            http.Response.StatusCode = StatusCodes.Status304NotModified;
            await http.Response.StartAsync(ct);
            return;
        }

        var blob = await store.OpenReadAsync(product.Id, id, ct);
        if (blob is null)
        {
            await http.Response.SendNotFoundAsync(ct);
            return;
        }

        var format = product.Images.Single(i => i.Id == id).Format;
        await Results.Stream(blob.Value.Content, format.ContentType()).ExecuteAsync(http);
    }
}

/// <summary>A Published Product's image, for anyone.</summary>
public sealed class GetMarketplaceImageEndpoint(CatalogDbContext db, IProductImageStore store) : Endpoint<ImageRequest>
{
    public override void Configure()
    {
        Get("/products/{productId}/images/{imageId}");
        Group<MarketplaceGroup>();
        Summary(s => s.Responses[404] = "No such image, or its Product isn't Published.");
    }

    public override async Task HandleAsync(ImageRequest req, CancellationToken ct)
    {
        var productId = ProductId.From(req.ProductId);
        var product = await db.Products.AsNoTracking().VisibleToBuyers().SingleOrDefaultAsync(p => p.Id == productId, ct);
        await ImageStreaming.SendAsync(HttpContext, store, product, req.ImageId, isPrivate: false, ct);
    }
}

/// <summary>One of the signed-in Merchant's own Product images, in any status.</summary>
public sealed class GetMerchantImageEndpoint(MerchantCatalog catalog, IProductImageStore store) : Endpoint<ImageRequest>
{
    public override void Configure()
    {
        Get("/products/{productId}/images/{imageId}");
        Group<MerchantCentreGroup>();
        Summary(s => s.Responses[404] = "No such image among the Merchant's own Products.");
    }

    public override async Task HandleAsync(ImageRequest req, CancellationToken ct)
    {
        var productId = ProductId.From(req.ProductId);
        var product = await catalog.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == productId, ct);
        await ImageStreaming.SendAsync(HttpContext, store, product, req.ImageId, isPrivate: true, ct);
    }
}

/// <summary>Any Product's image, for Platform Operators reviewing it.</summary>
public sealed class GetOperatorImageEndpoint(CatalogDbContext db, IProductImageStore store) : Endpoint<ImageRequest>
{
    public override void Configure()
    {
        Get("/products/{productId}/images/{imageId}");
        Group<OpsGroup>();
        Summary(s => s.Responses[404] = "No such image.");
    }

    public override async Task HandleAsync(ImageRequest req, CancellationToken ct)
    {
        var productId = ProductId.From(req.ProductId);
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == productId, ct);
        await ImageStreaming.SendAsync(HttpContext, store, product, req.ImageId, isPrivate: true, ct);
    }
}
