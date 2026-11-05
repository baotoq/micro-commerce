using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

public sealed record ProductVersionRequest(Guid Id, int? Version);

public sealed class ProductVersionValidator : Validator<ProductVersionRequest>
{
    public ProductVersionValidator() => RuleFor(x => x.Version).NotNull().GreaterThan(0).WithMessage("version is required.");
}

/// <summary>A Merchant publishes a Draft Product, so Buyers can find it (FR-008).</summary>
public sealed class PublishProductEndpoint(MerchantCatalog catalog) : Endpoint<ProductVersionRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id}/publish");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Publish a Product";
            s.Responses[404] = "No such Product among the Merchant's own.";
            s.Responses[409] = "product-version-conflict (stale version) or product-removed.";
        });
    }

    public override async Task HandleAsync(ProductVersionRequest req, CancellationToken ct)
    {
        var product = await this.LoadForChangeAsync(catalog, req.Id, req.Version, ct);
        if (product is null) return;

        product.Publish();
        if (!await ProductEndpointSupport.TrySaveAsync(HttpContext, catalog.Db, ct)) return;

        await Send.OkAsync(MerchantProductResponse.From(product), ct);
    }
}

/// <summary>A Merchant takes a Published Product back to Draft, hiding it from Buyers (FR-008).</summary>
public sealed class UnpublishProductEndpoint(MerchantCatalog catalog) : Endpoint<ProductVersionRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id}/unpublish");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Unpublish a Product";
            s.Responses[404] = "No such Product among the Merchant's own.";
            s.Responses[409] = "product-version-conflict (stale version) or product-removed.";
        });
    }

    public override async Task HandleAsync(ProductVersionRequest req, CancellationToken ct)
    {
        var product = await this.LoadForChangeAsync(catalog, req.Id, req.Version, ct);
        if (product is null) return;

        product.Unpublish();
        if (!await ProductEndpointSupport.TrySaveAsync(HttpContext, catalog.Db, ct)) return;

        await Send.OkAsync(MerchantProductResponse.From(product), ct);
    }
}
