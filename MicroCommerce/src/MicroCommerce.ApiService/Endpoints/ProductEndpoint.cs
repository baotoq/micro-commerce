using MicroCommerce.ApiService.UseCases.Products;

namespace MicroCommerce.ApiService.Endpoints;

public static class ProductEndpoint
{
    public static void MapProducts(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/products")
            .WithTags("products");
        
        group.MapGet("/", GetProductsQuery.EndpointHandler);
        group.MapGet("/es", SearchProductsFromEsQuery.EndpointHandler);
        group.MapGet("/{id}", GetProductQuery.EndpointHandler);
        group.MapPost("/", CreateProductCommand.EndpointHandler);
    }
}