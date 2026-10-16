using Riok.Mapperly.Abstractions;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>Maps Catalog module entities to their responses; every response member must have a source.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CatalogMapper
{
    public static partial CategoryResponse ToResponse(this Category category);

    public static partial IQueryable<CategoryResponse> ProjectToResponse(this IQueryable<Category> query);

    /// <summary>Maps the Category and, recursively, the <see cref="Category.Children"/> loaded into it.</summary>
    public static partial CategoryNodeResponse ToNodeResponse(this Category category);
}
