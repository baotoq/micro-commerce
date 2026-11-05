using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

/// <summary>Merchants upload, reorder and remove Product images (User Story 5, FR-020 to FR-023).</summary>
public class ProductImageEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Jpeg_png_and_webp_uploads_are_appended_in_order()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();

        await merchant.UploadOkAsync(product.Id, TestImages.Jpeg());
        await merchant.UploadOkAsync(product.Id, TestImages.Png());
        var withThree = await merchant.UploadOkAsync(product.Id, TestImages.WebP());

        withThree.Images.Select(i => i.Position).ShouldBe([0, 1, 2]);
        withThree.Images[0].Url.ShouldBe($"/api/merchant/products/{product.Id}/images/{withThree.Images[0].Id}");
        withThree.Version.ShouldBe(product.Version + 3);

        var image = await merchant.GetAsync(withThree.Images[2].Url, Ct);
        image.StatusCode.ShouldBe(HttpStatusCode.OK);
        image.Content.Headers.ContentType!.MediaType.ShouldBe("image/webp");
        (await image.Content.ReadAsByteArrayAsync(Ct)).ShouldBe(TestImages.WebP());
    }

    [Fact]
    public async Task Files_that_are_not_images_by_content_too_big_or_missing_are_refused_on_file()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();

        var text = await merchant.UploadAsync(product.Id, TestImages.Text(), "x.jpg", "image/jpeg");
        var tooBig = await merchant.UploadAsync(product.Id, TestImages.Png(5 * 1024 * 1024 + 1));
        using var empty = new MultipartFormDataContent { { new StringContent("nothing"), "other" } };
        var missing = await merchant.PostAsync($"/api/merchant/products/{product.Id}/images", empty, Ct);

        foreach (var response in new[] { text, tooBig, missing })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            (await response.ProblemAsync()).Errors!.Keys.ShouldBe(["file"]);
        }

        (await merchant.GetOwnProductAsync(product.Id)).Images.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_tenth_image_is_refused_and_leaves_the_images_unchanged()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        for (var i = 0; i < 9; i++) await merchant.UploadOkAsync(product.Id);

        var tenth = await merchant.UploadAsync(product.Id, TestImages.Png());

        tenth.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await tenth.ProblemAsync()).Type.ShouldBe("image-limit-reached");
        (await merchant.GetOwnProductAsync(product.Id)).Images.Count.ShouldBe(9);
    }

    [Fact]
    public async Task Of_two_parallel_uploads_at_eight_images_exactly_one_succeeds()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        for (var i = 0; i < 8; i++) await merchant.UploadOkAsync(product.Id);

        var responses = await Task.WhenAll(merchant.UploadAsync(product.Id, TestImages.Png()), merchant.UploadAsync(product.Id, TestImages.Jpeg()));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(1);
        (await merchant.GetOwnProductAsync(product.Id)).Images.Count.ShouldBe(9);
    }

    [Fact]
    public async Task Reordering_changes_the_cover_on_the_marketplace()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync(CatalogTestData.Unique("Vase"));
        await merchant.UploadOkAsync(product.Id, TestImages.Jpeg());
        await merchant.UploadOkAsync(product.Id, TestImages.Png());
        var three = await merchant.UploadOkAsync(product.Id, TestImages.WebP());
        var ids = three.Images.Select(i => i.Id).ToList();

        var response = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}/images/order",
            new { imageIds = new[] { ids[2], ids[0], ids[1] }, version = three.Version }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var reordered = (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
        reordered.Images.Select(i => i.Id).ShouldBe([ids[2], ids[0], ids[1]]);
        await merchant.PublishAsync(reordered);
        var detail = (await App.Client.GetFromJsonAsync<ProductDetailDto>($"/api/marketplace/products/{product.Id}", Ct))!;
        detail.Images.Select(i => i.Id).ShouldBe([ids[2], ids[0], ids[1]]);
        var card = (await App.Client.BrowseAsync($"shopId={(await merchant.GetFromJsonAsync<ShopProfileDto>("/api/merchant/shop", Ct))!.MerchantId}")).Items.Single();
        card.CoverImageUrl.ShouldBe($"/api/marketplace/products/{product.Id}/images/{ids[2]}");
    }

    [Fact]
    public async Task A_reorder_that_is_not_a_permutation_or_has_a_stale_version_is_refused()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        await merchant.UploadOkAsync(product.Id);
        var two = await merchant.UploadOkAsync(product.Id);

        var notAPermutation = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}/images/order",
            new { imageIds = new[] { two.Images[0].Id }, version = two.Version }, Ct);
        var stale = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}/images/order",
            new { imageIds = two.Images.Select(i => i.Id).Reverse().ToArray(), version = two.Version - 1 }, Ct);

        notAPermutation.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await notAPermutation.ProblemAsync()).Errors!.Keys.ShouldBe(["imageIds"]);
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await stale.ProblemAsync()).Type.ShouldBe("product-version-conflict");
    }

    [Fact]
    public async Task Removing_an_image_hides_it_and_renumbers_the_rest()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        await merchant.UploadOkAsync(product.Id);
        var two = await merchant.UploadOkAsync(product.Id);
        var removed = two.Images[0];

        var response = await merchant.DeleteAsync($"/api/merchant/products/{product.Id}/images/{removed.Id}?version={two.Version}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var after = (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
        after.Images.ShouldHaveSingleItem().Position.ShouldBe(0);
        (await merchant.GetAsync(removed.Url, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await merchant.DeleteAsync($"/api/merchant/products/{product.Id}/images/{Guid.NewGuid()}?version={after.Version}", Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Marketplace_images_revalidate_and_disappear_once_unpublished()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        var withImage = await merchant.UploadOkAsync(product.Id, TestImages.Jpeg());
        var url = $"/api/marketplace/products/{product.Id}/images/{withImage.Images[0].Id}";

        (await App.Client.GetAsync(url, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var published = await merchant.PublishAsync(withImage);

        var image = await App.Client.GetAsync(url, Ct);
        image.StatusCode.ShouldBe(HttpStatusCode.OK);
        image.Content.Headers.ContentType!.MediaType.ShouldBe("image/jpeg");
        image.Headers.GetValues("X-Content-Type-Options").ShouldBe(["nosniff"]);
        image.Headers.CacheControl!.NoCache.ShouldBeTrue();
        var etag = image.Headers.ETag.ShouldNotBeNull();

        using var revalidate = new HttpRequestMessage(HttpMethod.Get, url);
        revalidate.Headers.IfNoneMatch.Add(etag);
        (await App.Client.SendAsync(revalidate, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotModified);

        (await merchant.PostAsJsonAsync($"/api/merchant/products/{product.Id}/unpublish", new { version = published.Version }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
        (await App.Client.GetAsync(url, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Merchant_image_routes_are_private_to_the_owner()
    {
        var (owner, _, _) = await App.NewMerchantAsync();
        var (stranger, _, _) = await App.NewMerchantAsync();
        var product = await owner.CreateSimpleProductAsync();
        var withImage = await owner.UploadOkAsync(product.Id);
        var image = withImage.Images[0];
        var withoutMerchant = App.ClientFor(TestActor.Buyer());

        var own = await owner.GetAsync(image.Url, Ct);
        own.StatusCode.ShouldBe(HttpStatusCode.OK);
        own.Headers.CacheControl!.Private.ShouldBeTrue();

        foreach (var (client, expected) in new[] { (stranger, HttpStatusCode.NotFound), (withoutMerchant, HttpStatusCode.Forbidden), (App.Client, HttpStatusCode.Unauthorized) })
        {
            (await client.GetAsync(image.Url, Ct)).StatusCode.ShouldBe(expected);
            (await client.UploadAsync(product.Id, TestImages.Png())).StatusCode.ShouldBe(expected);
            (await client.PutAsJsonAsync($"/api/merchant/products/{product.Id}/images/order", new { imageIds = new[] { image.Id }, version = withImage.Version }, Ct))
                .StatusCode.ShouldBe(expected);
            (await client.DeleteAsync($"/api/merchant/products/{product.Id}/images/{image.Id}?version={withImage.Version}", Ct)).StatusCode.ShouldBe(expected);
        }

        (await owner.GetOwnProductAsync(product.Id)).Images.ShouldHaveSingleItem();
    }
}
