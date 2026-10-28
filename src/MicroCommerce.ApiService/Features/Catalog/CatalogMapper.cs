using Riok.Mapperly.Abstractions;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>Maps Catalog module entities to their responses; every response member must have a source.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CatalogMapper
{
    public static partial IQueryable<CategoryResponse> ProjectToResponse(this IQueryable<Category> query);

    /// <summary>Maps the Category and, recursively, the <see cref="Category.Children"/> loaded into it.</summary>
    public static partial CategoryNodeResponse ToNodeResponse(this Category category);

    public static partial CategoryResponse ToResponse(this Category category);

    [MapperIgnoreSource(nameof(Product.MerchantId))]
    [MapPropertyFromSource(nameof(ProductResponse.Variants), Use = nameof(OrderedVariants))]
    public static partial ProductResponse ToResponse(this Product product);

    [MapperIgnoreSource(nameof(Variant.Position))]
    private static partial VariantResponse ToVariantResponse(this Variant variant);

    [MapperIgnoreSource(nameof(Product.MerchantId))]
    [MapperIgnoreSource(nameof(Product.Description))]
    [MapperIgnoreSource(nameof(Product.Options))]
    [MapperIgnoreSource(nameof(Product.Variants))]
    [MapPropertyFromSource(nameof(ProductSummaryResponse.VariantCount), Use = nameof(VariantCount))]
    [MapPropertyFromSource(nameof(ProductSummaryResponse.MinPrice), Use = nameof(MinPrice))]
    [MapPropertyFromSource(nameof(ProductSummaryResponse.MaxPrice), Use = nameof(MaxPrice))]
    [MapPropertyFromSource(nameof(ProductSummaryResponse.TotalStock), Use = nameof(TotalStock))]
    public static partial ProductSummaryResponse ToSummaryResponse(this Product product);

    /// <summary>Uses <see cref="ToSummaryResponse"/>'s configuration; the database computes the aggregates.</summary>
    public static partial IQueryable<ProductSummaryResponse> ProjectToSummaryResponse(this IQueryable<Product> query);

    private static IReadOnlyList<VariantResponse> OrderedVariants(Product product) =>
        [.. product.Variants.OrderBy(v => v.Position).Select(v => v.ToVariantResponse())];

    // Expression-bodied, so Mapperly inlines them into the projection.
    private static int VariantCount(Product product) => product.Variants.Count;
    private static Money MinPrice(Product product) => product.Variants.Min(v => v.Price);
    private static Money MaxPrice(Product product) => product.Variants.Max(v => v.Price);
    private static long TotalStock(Product product) => product.Variants.Sum(v => (long)v.Stock);
}
