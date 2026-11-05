using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

/// <summary>A Merchant edits a Product it owns; the status stays (FR-008, FR-014, FR-019).</summary>
public sealed class UpdateProductEndpoint(MerchantCatalog catalog) : Endpoint<UpdateProductRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Put("/products/{id}");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Edit a Product";
            s.Responses[400] = "A field is invalid, or categoryId isn't an existing leaf Category.";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "merchant-required: the Account owns no Merchant.";
            s.Responses[404] = "No such Product among the Merchant's own.";
            s.Responses[409] = "product-version-conflict (stale version) or product-removed.";
        });
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var product = await this.LoadForChangeAsync(catalog, req.Id, req.Version, ct);
        if (product is null) return;

        var (options, variants) = req.ToDomain();
        var categoryId = CategoryId.From(req.CategoryId);
        var moving = product.CategoryId != categoryId;
        product.Edit(req.Name, req.Description, categoryId, options, variants);

        if (moving)
        {
            try
            {
                if (!await ProductEndpointSupport.SaveInLeafAsync(catalog.Db, categoryId, () => { }, ct))
                {
                    AddError(r => r.CategoryId, ProductEndpointSupport.NotALeaf);
                    await Send.ErrorsAsync(cancellation: ct);
                    return;
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                await Send.ResultAsync(Infrastructure.Problems.Conflict(Infrastructure.Problems.ProductVersionConflict));
                return;
            }
        }
        else if (!await ProductEndpointSupport.TrySaveAsync(HttpContext, catalog.Db, ct))
        {
            return;
        }

        await Send.OkAsync(MerchantProductResponse.From(product), ct);
    }
}
