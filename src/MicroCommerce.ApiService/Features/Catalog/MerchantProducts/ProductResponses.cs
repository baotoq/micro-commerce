using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

public sealed record OptionResponse(string Name, IReadOnlyList<string> Values);

public sealed record VariantResponse(Guid Id, IReadOnlyList<string> OptionValues, long Price, int Stock, int WeightGrams);

public sealed record ImageRef(Guid Id, int Position, string Url);

/// <summary>A Product as its Merchant (or a Platform Operator) sees it: stock and weight included.</summary>
public sealed record MerchantProductResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    string Status,
    IReadOnlyList<OptionResponse> Options,
    IReadOnlyList<VariantResponse> Variants,
    IReadOnlyList<ImageRef> Images,
    DateTimeOffset CreatedAt,
    int Version)
{
    /// <param name="imageBase">Where this caller fetches images: /api/merchant or /api/ops.</param>
    public static MerchantProductResponse From(Product p, string imageBase = "/api/merchant") => new(
        p.Id.Value,
        p.Name,
        p.Description,
        p.CategoryId.Value,
        p.Status.ToString(),
        p.Options.Select(o => new OptionResponse(o.Name, o.Values)).ToList(),
        p.Variants.Select(v => new VariantResponse(v.Id.Value, v.OptionValues, v.Price.Value, v.Stock, v.WeightGrams)).ToList(),
        p.Images.Select(i => new ImageRef(i.Id.Value, i.Position, ImageUrl(imageBase, p.Id, i.Id))).ToList(),
        p.CreatedAt,
        p.Version);

    public static string ImageUrl(string imageBase, ProductId productId, ProductImageId imageId) =>
        $"{imageBase}/products/{productId.Value}/images/{imageId.Value}";
}

/// <summary>One row of the Merchant's own Product list (FR-015).</summary>
public sealed record MerchantProductSummary(
    Guid Id,
    string Name,
    Guid CategoryId,
    string Status,
    int VariantCount,
    long MinPrice,
    long MaxPrice,
    long TotalStock,
    string? CoverImageUrl,
    DateTimeOffset CreatedAt);
