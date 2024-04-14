using MicroCommerce.ApiService.UseCases.Categories;

namespace MicroCommerce.ApiService.Endpoints;

public static class CartEndCategoryEndpoint
{
    public static void MapCategories(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/categories")
            .WithTags("categories");
        
        group.MapGet("/", GetCategoriesQuery.EndpointHandler);
    }
}
