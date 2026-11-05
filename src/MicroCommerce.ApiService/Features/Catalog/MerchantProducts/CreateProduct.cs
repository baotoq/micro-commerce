using FastEndpoints;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

/// <summary>A Merchant lists a new Product. It starts as Draft (FR-007, FR-008).</summary>
public sealed class CreateProductEndpoint(
    MerchantCatalog catalog,
    ICurrentMerchant currentMerchant,
    TimeProvider timeProvider,
    ILogger<CreateProductEndpoint> logger) : Endpoint<CreateProductRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Post("/products");
        Group<MerchantCentreGroup>();
        Summary(s =>
        {
            s.Summary = "Create a Product (Draft)";
            s.Responses[201] = "The Product was created as Draft.";
            s.Responses[400] = "A field is invalid, or categoryId isn't an existing leaf Category.";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "merchant-required: the Account owns no Merchant.";
        });
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var (options, variants) = req.ToDomain();
        var categoryId = CategoryId.From(req.CategoryId);
        var product = Product.Create(
            ProductId.New(), currentMerchant.Id, req.Name, req.Description, categoryId, options, variants, timeProvider.GetUtcNow());

        if (!await ProductEndpointSupport.SaveInLeafAsync(catalog.Db, categoryId, () => catalog.Db.Products.Add(product), ct))
        {
            AddError(r => r.CategoryId, ProductEndpointSupport.NotALeaf);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        logger.LogInformation("Product {ProductId} created by Merchant {MerchantId}", product.Id, currentMerchant.Id);
        HttpContext.Response.Headers.Location = $"/api/merchant/products/{product.Id.Value}";
        await Send.ResponseAsync(MerchantProductResponse.From(product), StatusCodes.Status201Created, ct);
    }
}
