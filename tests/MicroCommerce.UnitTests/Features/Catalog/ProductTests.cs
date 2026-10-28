using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class ProductTests
{
    private static Product NewProduct() =>
        new() { MerchantId = MerchantId.New(), Name = "Coffee Mug", CategoryId = CategoryId.New() };

    [Fact]
    public void New_product_is_a_draft_without_variants_and_gets_a_version_7_id()
    {
        var product = NewProduct();

        product.Status.ShouldBe(ProductStatus.Draft);
        product.Variants.ShouldBeEmpty();
        product.Id.Value.Version.ShouldBe(7);
    }

    [Fact]
    public void Setting_the_default_variant_creates_one()
    {
        var product = NewProduct();

        product.SetDefaultVariant(Money.From(120_000), 5, 350);

        var variant = product.Variants.ShouldHaveSingleItem();
        variant.Id.Value.Version.ShouldBe(7);
        (variant.Price, variant.Stock, variant.WeightGrams).ShouldBe((Money.From(120_000), 5, 350));
    }

    [Fact]
    public void Setting_the_default_variant_again_keeps_its_id()
    {
        var product = NewProduct();
        product.SetDefaultVariant(Money.From(120_000), 5, 350);
        var id = product.Variants.Single().Id;

        product.SetDefaultVariant(Money.From(150_000), 0, 400);

        var variant = product.Variants.ShouldHaveSingleItem();
        variant.Id.ShouldBe(id);
        (variant.Price, variant.Stock, variant.WeightGrams).ShouldBe((Money.From(150_000), 0, 400));
    }

    [Fact]
    public void Publish_and_unpublish_move_between_draft_and_published()
    {
        var product = NewProduct();

        product.Publish();
        product.Status.ShouldBe(ProductStatus.Published);
        product.Publish();
        product.Status.ShouldBe(ProductStatus.Published);

        product.Unpublish();
        product.Status.ShouldBe(ProductStatus.Draft);
    }
}
