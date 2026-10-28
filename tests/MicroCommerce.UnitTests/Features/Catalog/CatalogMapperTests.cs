using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class CatalogMapperTests
{
    private static Product NewProduct()
    {
        var product = new Product
        {
            MerchantId = MerchantId.New(),
            Name = "Coffee Mug",
            Description = "Stoneware.",
            CategoryId = CategoryId.New(),
        };
        product.SetDefaultVariant(Money.From(120_000), 5, 350);
        return product;
    }

    [Fact]
    public void Product_response_maps_all_fields()
    {
        var product = NewProduct();
        var variant = product.Variants.Single();

        var response = product.ToResponse();

        response.Variants.ShouldBe([new VariantResponse(variant.Id, Money.From(120_000), 5, 350)]);
        (response with { Variants = [] }).ShouldBeEquivalentTo(new ProductResponse(
            product.Id, "Coffee Mug", "Stoneware.", product.CategoryId, ProductStatus.Draft, [], product.CreatedAt));
    }

    [Fact]
    public void Summary_projection_computes_price_range_and_total_stock()
    {
        var product = NewProduct();
        product.Publish();

        var responses = new[] { product }.AsQueryable().ProjectToSummaryResponse().ToList();

        responses.ShouldBe([new ProductSummaryResponse(
            product.Id, "Coffee Mug", product.CategoryId, ProductStatus.Published, 1,
            Money.From(120_000), Money.From(120_000), 5, product.CreatedAt)]);
    }
}
