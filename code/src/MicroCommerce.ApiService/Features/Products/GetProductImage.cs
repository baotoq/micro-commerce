using System.Net.Mime;
using Ardalis.GuardClauses;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Products;

public class GetProductImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/products/images/{url}", async (string url, IFileService fileService) =>
        {
            var stream = await fileService.DownloadFileAsync(url);

            return TypedResults.File(stream, MediaTypeNames.Image.Jpeg);
        }).Produces<Stream>(contentType: MediaTypeNames.Image.Jpeg);
    }
}
