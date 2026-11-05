using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.ProductImages;

public sealed record RemoveImageRequest(Guid Id, Guid ImageId, [property: QueryParam] int? Version);

public sealed class RemoveImageValidator : Validator<RemoveImageRequest>
{
    public RemoveImageValidator() => RuleFor(x => x.Version).NotNull().GreaterThan(0).WithMessage("version is required.");
}

/// <summary>A Merchant removes one of its Product's images. The Product is saved first, then the blob is deleted.</summary>
public sealed class RemoveImageEndpoint(MerchantCatalog catalog, IProductImageStore store, ILogger<RemoveImageEndpoint> logger)
    : Endpoint<RemoveImageRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Delete("/products/{id}/images/{imageId}");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Remove a Product image";
            s.Responses[404] = "No such Product among the Merchant's own, or no such image.";
            s.Responses[409] = "product-version-conflict (stale version) or product-removed.";
        });
    }

    public override async Task HandleAsync(RemoveImageRequest req, CancellationToken ct)
    {
        var product = await this.LoadForChangeAsync(catalog, req.Id, req.Version, ct);
        if (product is null) return;

        var imageId = ProductImageId.From(req.ImageId);
        if (!product.HasImage(imageId))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        product.RemoveImage(imageId);
        if (!await ProductEndpointSupport.TrySaveAsync(HttpContext, catalog.Db, ct)) return;

        // Unreferenced now, so never served again even if the delete fails (research R7).
        if (!await store.TryDeleteAsync(product.Id, imageId, CancellationToken.None))
            logger.LogWarning("Orphaned image blob {ProductId}/{ImageId} could not be deleted", product.Id, imageId);

        await Send.OkAsync(MerchantProductResponse.From(product), ct);
    }
}
