using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Catalog;

public record CreateProductRequest(
    string Name,
    string? Description,
    CategoryId? CategoryId,
    IReadOnlyList<ProductOption>? Options,
    IReadOnlyList<VariantRequest> Variants) : IProductDetails;

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        Include(new ProductDetailsValidator());
    }
}

/// <summary>
/// Lists a new Product, as Draft, in a leaf Category, with a Variant for every combination of its options' values
/// (or a single default Variant without options).
/// </summary>
public class CreateProductEndpoint(CatalogDbContext db, MerchantCatalog catalog) : Endpoint<CreateProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Post("/products");
        Group<MerchantCentre>();
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = new Product
        {
            MerchantId = catalog.MerchantId,
            Name = req.Name,
            CategoryId = req.CategoryId!.Value,
        };
        req.ApplyTo(product);
        db.Products.Add(product);

        if (await db.SaveInLeafCategoryAsync(product.CategoryId, ct) is { } reason)
        {
            AddError(r => r.CategoryId, reason);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.CreatedAtAsync<GetProductEndpoint>(new { id = product.Id }, product.ToResponse(), cancellation: ct);
    }
}
