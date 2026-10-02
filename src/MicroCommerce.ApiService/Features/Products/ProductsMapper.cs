using Riok.Mapperly.Abstractions;

namespace MicroCommerce.ApiService.Features.Products;

/// <summary>Maps Products module entities to their responses; every response member must have a source.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ProductsMapper
{
    public static partial ProductResponse ToResponse(this Product product);

    public static partial IQueryable<ProductResponse> ProjectToResponse(this IQueryable<Product> query);
}
