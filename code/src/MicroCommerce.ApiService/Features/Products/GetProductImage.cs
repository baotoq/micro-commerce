using Ardalis.GuardClauses;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Products;

public class GetProductImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/products/images/{url}", async (string url, IMediator mediator, IWebHostEnvironment environment) =>
        {
            var path = Path.Combine(environment.ContentRootPath, "Resources/Images", url);

            if (!File.Exists(path))
            {
                return Results.NotFound();
            }

            return Results.File(path, "image/jpeg");
        });
    }
}
