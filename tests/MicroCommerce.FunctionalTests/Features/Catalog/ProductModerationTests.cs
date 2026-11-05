using System.Net;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

public sealed record OperatorProductDto(
    Guid Id, string Name, string Status, Guid MerchantId, string? ShopName, List<VariantDto> Variants, List<ImageDto> Images, int Version);

/// <summary>Platform Operators view any Product and remove prohibited ones (User Story 7, FR-037 to FR-039).</summary>
public class ProductModerationTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient Operator => App.ClientFor(TestActor.PlatformOperator());

    [Fact]
    public async Task An_operator_views_any_merchants_draft()
    {
        var (merchant, shop, _) = await App.NewMerchantAsync();
        var draft = await merchant.CreateSimpleProductAsync();
        await merchant.UploadOkAsync(draft.Id);

        var product = (await Operator.GetFromJsonAsync<OperatorProductDto>($"/api/ops/products/{draft.Id}", Ct))!;

        product.Status.ShouldBe("Draft");
        product.MerchantId.ShouldBe(shop.MerchantId);
        product.ShopName.ShouldBe(shop.ShopName);
        var image = product.Images.ShouldHaveSingleItem();
        image.Url.ShouldStartWith($"/api/ops/products/{draft.Id}/images/");
        (await Operator.GetAsync(image.Url, Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Operator.GetAsync($"/api/ops/products/{Guid.NewGuid()}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Removing_takes_a_product_off_the_marketplace_at_once_and_is_idempotent()
    {
        var token = $"tok{Guid.NewGuid():N}"[..20];
        var (merchant, shop, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync($"{token} knife");
        var withImage = await merchant.UploadOkAsync(product.Id);
        await merchant.PublishAsync(withImage);
        var imageUrl = $"/api/marketplace/products/{product.Id}/images/{withImage.Images[0].Id}";
        (await App.Client.BrowseAsync($"query={token}")).Items.ShouldHaveSingleItem();

        var first = await Operator.PostAsync($"/api/ops/products/{product.Id}/remove", null, Ct);
        var again = await Operator.PostAsync($"/api/ops/products/{product.Id}/remove", null, Ct);

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await first.Content.ReadFromJsonAsync<OperatorProductDto>(Ct))!.Status.ShouldBe("Removed");
        (await App.Client.BrowseAsync($"query={token}")).Items.ShouldBeEmpty();
        (await App.Client.BrowseAsync($"shopId={shop.MerchantId}")).Items.ShouldBeEmpty();
        (await App.Client.GetAsync($"/api/marketplace/products/{product.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await App.Client.GetAsync(imageUrl, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task The_merchant_sees_a_removed_product_but_cannot_change_it()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.UploadOkAsync((await merchant.CreateSimpleProductAsync()).Id);
        (await Operator.PostAsync($"/api/ops/products/{product.Id}/remove", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var removed = await merchant.GetOwnProductAsync(product.Id);
        var image = removed.Images.Single();

        removed.Status.ShouldBe("Removed");
        (await merchant.GetFromJsonAsync<PageDto<MerchantProductSummaryDto>>("/api/merchant/products", Ct))!.Items.Single().Status.ShouldBe("Removed");

        var attempts = new[]
        {
            await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", MerchantProductEndpointsTests.UpdateBody(removed, name: "Back"), Ct),
            await merchant.PostAsJsonAsync($"/api/merchant/products/{product.Id}/publish", new { version = removed.Version }, Ct),
            await merchant.PostAsJsonAsync($"/api/merchant/products/{product.Id}/unpublish", new { version = removed.Version }, Ct),
            await merchant.UploadAsync(product.Id, TestImages.Png()),
            await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}/images/order", new { imageIds = new[] { image.Id }, version = removed.Version }, Ct),
            await merchant.DeleteAsync($"/api/merchant/products/{product.Id}/images/{image.Id}?version={removed.Version}", Ct),
        };

        foreach (var response in attempts)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await response.ProblemAsync()).Type.ShouldBe("product-removed");
        }
    }

    [Fact]
    public async Task A_removal_racing_a_merchant_edit_always_ends_removed()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();

        for (var i = 0; i < 5; i++)
        {
            var product = await merchant.CreateSimpleProductAsync();
            await Task.WhenAll(
                Operator.PostAsync($"/api/ops/products/{product.Id}/remove", null, Ct),
                merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", MerchantProductEndpointsTests.UpdateBody(product, name: "Racing"), Ct));

            (await merchant.GetOwnProductAsync(product.Id)).Status.ShouldBe("Removed");
        }
    }

    [Fact]
    public async Task Only_platform_operators_moderate()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();

        foreach (var (client, expected) in new[] { (App.ClientFor(TestActor.Buyer()), HttpStatusCode.Forbidden), (merchant, HttpStatusCode.Forbidden), (App.Client, HttpStatusCode.Unauthorized) })
        {
            (await client.GetAsync($"/api/ops/products/{product.Id}", Ct)).StatusCode.ShouldBe(expected);
            (await client.PostAsync($"/api/ops/products/{product.Id}/remove", null, Ct)).StatusCode.ShouldBe(expected);
            (await client.GetAsync($"/api/ops/products/{product.Id}/images/{Guid.NewGuid()}", Ct)).StatusCode.ShouldBe(expected);
        }

        (await merchant.GetOwnProductAsync(product.Id)).Status.ShouldBe("Draft");
    }
}
