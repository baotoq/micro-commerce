using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.ProductImages;

public sealed record ReorderImagesRequest(Guid Id, List<Guid> ImageIds, int? Version);

public sealed class ReorderImagesValidator : Validator<ReorderImagesRequest>
{
    public ReorderImagesValidator()
    {
        RuleFor(x => x.ImageIds).NotNull().WithMessage("imageIds is required.");
        RuleFor(x => x.Version).NotNull().GreaterThan(0).WithMessage("version is required.");
    }
}

/// <summary>A Merchant puts its Product's images in a new order; the first is the cover (FR-021).</summary>
public sealed class ReorderImagesEndpoint(MerchantCatalog catalog) : Endpoint<ReorderImagesRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Put("/products/{id}/images/order");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Reorder a Product's images";
            s.Responses[400] = "imageIds isn't the current images in some order.";
            s.Responses[404] = "No such Product among the Merchant's own.";
            s.Responses[409] = "product-version-conflict (stale version) or product-removed.";
        });
    }

    public override async Task HandleAsync(ReorderImagesRequest req, CancellationToken ct)
    {
        var product = await this.LoadForChangeAsync(catalog, req.Id, req.Version, ct);
        if (product is null) return;

        var current = product.Images.Select(i => i.Id.Value).ToHashSet();
        if (req.ImageIds.Count != current.Count || !current.SetEquals(req.ImageIds) || req.ImageIds.Distinct().Count() != req.ImageIds.Count)
        {
            AddError(r => r.ImageIds, "imageIds must list each of the Product's images exactly once.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        product.ReorderImages(req.ImageIds.Select(ProductImageId.From).ToList());
        if (!await ProductEndpointSupport.TrySaveAsync(HttpContext, catalog.Db, ct)) return;

        await Send.OkAsync(MerchantProductResponse.From(product), ct);
    }
}
