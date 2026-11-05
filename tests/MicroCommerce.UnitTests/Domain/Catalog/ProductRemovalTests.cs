using MicroCommerce.Domain.Catalog;
using static MicroCommerce.UnitTests.Domain.Catalog.ProductTests;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class ProductRemovalTests
{
    [Fact]
    public void A_draft_or_published_product_can_be_removed_and_the_version_moves_on()
    {
        var draft = Simple();
        var published = Simple();
        published.Publish();

        draft.Remove();
        published.Remove();

        draft.Status.ShouldBe(ProductStatus.Removed);
        draft.Version.ShouldBe(2);
        published.Status.ShouldBe(ProductStatus.Removed);
        published.Version.ShouldBe(3);
    }

    [Fact]
    public void Removing_twice_changes_nothing()
    {
        var product = Simple();
        product.Remove();

        product.Remove();

        product.Version.ShouldBe(2);
    }

    [Fact]
    public void A_removed_product_refuses_every_merchant_change()
    {
        var product = Simple();
        product.AddImage(ProductImageId.New(), ImageFormat.Png, 100, Now);
        var image = product.Images.Single().Id;
        product.Remove();

        Should.Throw<ProductRemovedException>(() => product.Edit("New", null, Kitchen, [], [Spec()]));
        Should.Throw<ProductRemovedException>(product.Publish);
        Should.Throw<ProductRemovedException>(product.Unpublish);
        Should.Throw<ProductRemovedException>(() => product.AddImage(ProductImageId.New(), ImageFormat.Png, 100, Now));
        Should.Throw<ProductRemovedException>(() => product.ReorderImages([image]));
        Should.Throw<ProductRemovedException>(() => product.RemoveImage(image));
        product.Status.ShouldBe(ProductStatus.Removed);
    }
}
