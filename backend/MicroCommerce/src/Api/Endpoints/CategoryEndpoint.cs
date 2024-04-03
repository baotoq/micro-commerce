using Api.UseCases.Categories;

namespace Api.Endpoints;

public static class CartEndCategoryEndpoint
{
    public static void MapCategories(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/categories")
            .WithTags("categories");
        
        group.MapGet("/", GetCategoriesQuery.EndpointHandler);
    }
}
