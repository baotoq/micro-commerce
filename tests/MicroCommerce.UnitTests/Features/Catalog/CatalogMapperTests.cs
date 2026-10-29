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

    [Fact]
    public void Storefront_response_carries_the_shop_and_variants_in_position_order_without_weight()
    {
        var product = NewProduct();
        var shop = new Shop { Id = product.MerchantId, ShopName = "Mug Corner", Description = "Mugs.", ProfileVersion = 2 };
        var (small, medium) = (product.Variants[0], product.Variants[1]);
        product.Variants.Reverse();

        var response = new ShopProduct { Product = product, Shop = shop }.ToStorefrontResponse();

        response.Options.ShouldBe([Size]);
        response.Variants.Select(v => (v.Id, string.Join(",", v.OptionValues), v.Price, v.Stock)).ShouldBe([
            (small.Id, "S", Money.From(120_000), 5),
            (medium.Id, "M", Money.From(150_000), 2),
        ]);
        (response with { Options = [], Variants = [] }).ShouldBeEquivalentTo(new StorefrontProductResponse(
            product.Id, "T-Shirt", "Cotton.", product.CategoryId, [], [], shop.Id, "Mug Corner"));
    }

    [Fact]
    public void Storefront_summary_projection_computes_price_range_and_carries_stock_and_the_shop()
    {
        var inStock = NewProduct();
        var soldOut = NewProduct();
        soldOut.SetVariants([], [new([], Money.From(90_000), 0, 100)]);
        var shop = new Shop { Id = inStock.MerchantId, ShopName = "Mug Corner" };

        // InStock depends on Reservations, so VisibleProducts works it out in the query; the mapper carries it.
        var responses = new[] { inStock, soldOut }
            .Select(p => new ShopProduct { Product = p, Shop = shop, InStock = p == inStock })
            .AsQueryable()
            .ProjectToStorefrontSummaryResponse()
            .ToList();

        responses.ShouldBe([
            new StorefrontProductSummaryResponse(
                inStock.Id, "T-Shirt", inStock.CategoryId, Money.From(120_000), Money.From(150_000), true, shop.Id, "Mug Corner"),
            new StorefrontProductSummaryResponse(
                soldOut.Id, "T-Shirt", soldOut.CategoryId, Money.From(90_000), Money.From(90_000), false, shop.Id, "Mug Corner"),
        ]);
    }
}
