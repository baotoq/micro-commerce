using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

public record UpdateProductRequest(
    ProductId Id,
    string Name,
    string? Description,
    CategoryId? CategoryId,
    IReadOnlyList<ProductOption>? Options,
    IReadOnlyList<VariantRequest> Variants) : IProductDetails;

public class UpdateProductValidator : Validator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        Include(new ProductDetailsValidator());
    }
}

/// <summary>
/// Replaces a Product's details, options and Variants. The Product keeps its status, and every option-value
/// combination it already had keeps its Variant ID.
/// </summary>
public class UpdateProductEndpoint(CatalogDbContext db, MerchantCatalog catalog) : Endpoint<UpdateProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Put("/products/{id:guid}");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var product = await catalog.Products.SingleOrDefaultAsync(p => p.Id == req.Id, ct);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        req.ApplyTo(product);
        product.CategoryId = req.CategoryId!.Value;

        if (await db.SaveInLeafCategoryAsync(product.CategoryId, ct) is { } reason)
        {
            AddError(r => r.CategoryId, reason);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(product.ToResponse(), ct);
    }
}
