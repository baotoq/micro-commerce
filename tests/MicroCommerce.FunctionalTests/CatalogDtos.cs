using System.Net;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests;

public sealed record OptionDto(string Name, List<string> Values);

public sealed record VariantDto(Guid Id, List<string> OptionValues, long Price, int Stock, int WeightGrams);

public sealed record ImageDto(Guid Id, int Position, string Url);

public sealed record MerchantProductDto(
    Guid Id, string Name, string? Description, Guid CategoryId, string Status,
    List<OptionDto> Options, List<VariantDto> Variants, List<ImageDto> Images, DateTimeOffset CreatedAt, int Version);

public sealed record MerchantProductSummaryDto(
    Guid Id, string Name, Guid CategoryId, string Status, int VariantCount, long MinPrice, long MaxPrice,
    long TotalStock, string? CoverImageUrl, DateTimeOffset CreatedAt);

public sealed record PageDto<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public static class ProductHelpers
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static object VariantRequest(long price = 120_000, int stock = 10, int weightGrams = 400, params string[] optionValues) =>
        new { optionValues, price, stock, weightGrams };

    public static object ProductRequest(
        string name, Guid categoryId, string? description = "350 ml, dishwasher safe",
        object[]? options = null, object[]? variants = null) =>
        new { name, description, categoryId, options = options ?? [], variants = variants ?? [VariantRequest()] };

    public static async Task<MerchantProductDto> CreateProductAsync(this HttpClient merchant, object request)
    {
        var response = await merchant.PostAsJsonAsync("/api/merchant/products", request, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Ct));
        return (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
    }

    /// <summary>A simple single-Variant Product in the seeded Home › Kitchen.</summary>
    public static async Task<MerchantProductDto> CreateSimpleProductAsync(this HttpClient merchant, string? name = null, long price = 120_000, int stock = 10)
    {
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(merchant);
        return await merchant.CreateProductAsync(ProductRequest(name ?? CatalogTestData.Unique("Mug"), kitchen,
            variants: [VariantRequest(price: price, stock: stock)]));
    }

    public static async Task<MerchantProductDto> PublishAsync(this HttpClient merchant, MerchantProductDto product)
    {
        var response = await merchant.PostAsJsonAsync($"/api/merchant/products/{product.Id}/publish", new { version = product.Version }, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Ct));
        return (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
    }

    public static async Task<MerchantProductDto> GetOwnProductAsync(this HttpClient merchant, Guid id) =>
        (await merchant.GetFromJsonAsync<MerchantProductDto>($"/api/merchant/products/{id}", Ct))!;
}

public sealed record ShopRefDto(Guid Id, string ShopName);

public sealed record ProductCardDto(
    Guid Id, string Name, Guid CategoryId, long MinPrice, long MaxPrice, bool Available, string? CoverImageUrl, ShopRefDto Shop);

public sealed record CategoryRefDto(Guid Id, string Name);

public sealed record MarketplaceVariantDto(Guid Id, List<string> OptionValues, long Price, int Availability);

public sealed record MarketplaceImageDto(Guid Id, string Url);

public sealed record ProductDetailDto(
    Guid Id, string Name, string? Description, CategoryRefDto Category, List<OptionDto> Options,
    List<MarketplaceVariantDto> Variants, List<MarketplaceImageDto> Images, ShopRefDto Shop);

public sealed record ShopPageDto(Guid Id, string ShopName, string? Description);

public static class MarketplaceHelpers
{
    public static async Task<PageDto<ProductCardDto>> BrowseAsync(this HttpClient client, string query) =>
        (await client.GetFromJsonAsync<PageDto<ProductCardDto>>($"/api/marketplace/products?{query}", TestContext.Current.CancellationToken))!;
}
