using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;
using static MicroCommerce.UnitTests.Domain.Catalog.ProductTests;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class ProductImageTests
{
    private static ProductImageId Add(Product product, long size = 1_000)
    {
        var id = ProductImageId.New();
        product.AddImage(id, ImageFormat.Png, size, Now);
        return id;
    }

    [Fact]
    public void Images_are_appended_in_order_and_each_bumps_the_version()
    {
        var product = Simple();

        var first = Add(product);
        var second = Add(product);

        product.Images.Select(i => (i.Id, i.Position)).ShouldBe([(first, 0), (second, 1)]);
        product.Version.ShouldBe(3);
        var image = product.Images[0];
        (image.Format, image.SizeBytes, image.UploadedAt).ShouldBe((ImageFormat.Png, 1_000L, Now));
    }

    [Fact]
    public void A_tenth_image_is_refused()
    {
        var product = Simple();
        for (var i = 0; i < 9; i++) Add(product);

        Should.Throw<ImageLimitReachedException>(() => Add(product));
        product.Images.Count.ShouldBe(9);
    }

    [Fact]
    public void An_image_over_5_mb_is_refused()
    {
        var product = Simple();
        Add(product, ProductImage.MaxSizeBytes);

        Should.Throw<DomainRuleViolation>(() => Add(product, ProductImage.MaxSizeBytes + 1));
    }

    [Fact]
    public void Reordering_takes_a_permutation_and_renumbers()
    {
        var product = Simple();
        var a = Add(product);
        var b = Add(product);
        var c = Add(product);

        product.ReorderImages([c, a, b]);

        product.Images.Select(i => i.Id).ShouldBe([c, a, b]);
        product.Images.Select(i => i.Position).ShouldBe([0, 1, 2]);
        product.Version.ShouldBe(5);
    }

    [Fact]
    public void Reordering_with_anything_but_a_permutation_is_refused()
    {
        var product = Simple();
        var a = Add(product);
        var b = Add(product);

        Should.Throw<DomainRuleViolation>(() => product.ReorderImages([a]));
        Should.Throw<DomainRuleViolation>(() => product.ReorderImages([a, a]));
        Should.Throw<DomainRuleViolation>(() => product.ReorderImages([a, b, ProductImageId.New()]));
    }

    [Fact]
    public void Removing_an_image_renumbers_the_rest()
    {
        var product = Simple();
        var a = Add(product);
        var b = Add(product);
        var c = Add(product);

        product.RemoveImage(b);

        product.Images.Select(i => (i.Id, i.Position)).ShouldBe([(a, 0), (c, 1)]);
        product.Version.ShouldBe(5);
        Should.Throw<DomainRuleViolation>(() => product.RemoveImage(b));
    }
}
