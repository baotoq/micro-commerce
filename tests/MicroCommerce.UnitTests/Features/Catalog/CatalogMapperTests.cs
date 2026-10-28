using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class CatalogMapperTests
{
    private static readonly ProductOption Size = new("Size", ["S", "M"]);

    private static Product NewProduct()
    {
        var product = new Product
        {
            MerchantId = MerchantId.New(),
            Name = "T-Shirt",
            Description = "Cotton.",
            CategoryId = CategoryId.New(),
        };
        product.SetVariants([Size], [new(["S"], Money.From(120_000), 5, 200), new(["M"], Money.From(150_000), 2, 250)]);
        return product;
    }

    [Fact]
    public void Product_response_maps_all_fields_with_variants_in_position_order()
    {
        var product = NewProduct();
        var (small, medium) = (product.Variants[0], product.Variants[1]);
        product.Variants.Reverse();

        var response = product.ToResponse();

        response.Options.ShouldBe([Size]);
        response.Variants.Select(v => (v.Id, string.Join(",", v.OptionValues), v.Price, v.Stock, v.WeightGrams)).ShouldBe([
            (small.Id, "S", Money.From(120_000), 5, 200),
            (medium.Id, "M", Money.From(150_000), 2, 250),
        ]);
        (response with { Options = [], Variants = [] }).ShouldBeEquivalentTo(new ProductResponse(
            product.Id, "T-Shirt", "Cotton.", product.CategoryId, ProductStatus.Draft, [], [], product.CreatedAt));
    }

    [Fact]
    public void Summary_projection_computes_price_range_and_total_stock_across_variants()
    {
        var product = NewProduct();
        product.Publish();

        var responses = new[] { product }.AsQueryable().ProjectToSummaryResponse().ToList();

        responses.ShouldBe([new ProductSummaryResponse(
            product.Id, "T-Shirt", product.CategoryId, ProductStatus.Published, 2,
            Money.From(120_000), Money.From(150_000), 7, product.CreatedAt)]);
    }
}
