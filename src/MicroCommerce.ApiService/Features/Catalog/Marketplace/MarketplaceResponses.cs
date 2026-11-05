using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.Marketplace;

/// <summary>The Shop that sells a Product.</summary>
public sealed record ShopSummary(Guid Id, string ShopName);

/// <summary>A Product in a Marketplace list: no stock, no weight, no Merchant internals.</summary>
public sealed record ProductCard(
    Guid Id, string Name, Guid CategoryId, long MinPrice, long MaxPrice, bool Available, string? CoverImageUrl, ShopSummary Shop);

public sealed record CategoryRef(Guid Id, string Name);

/// <summary>A Variant as Buyers see it: Availability, never stock (FR-030).</summary>
public sealed record MarketplaceVariant(Guid Id, IReadOnlyList<string> OptionValues, long Price, int Availability);

public sealed record MarketplaceImage(Guid Id, string Url);

public sealed record MarketplaceOption(string Name, IReadOnlyList<string> Values);

public sealed record ProductDetail(
    Guid Id,
    string Name,
    string? Description,
    CategoryRef Category,
    IReadOnlyList<MarketplaceOption> Options,
    IReadOnlyList<MarketplaceVariant> Variants,
    IReadOnlyList<MarketplaceImage> Images,
    ShopSummary Shop);

/// <summary>A Shop page: never the Pickup Address (FR-031).</summary>
public sealed record ShopPageResponse(Guid Id, string ShopName, string? Description);

public static class MarketplaceUrls
{
    public static string Image(ProductId productId, ProductImageId imageId) =>
        $"/api/marketplace/products/{productId.Value}/images/{imageId.Value}";
}
