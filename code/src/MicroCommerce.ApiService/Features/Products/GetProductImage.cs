using System.Net.Mime;
using Ardalis.GuardClauses;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Products;

public class GetProductImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/products/images/{url}",
            [OutputCache(Duration = 15, VaryByRouteValueNames = ["url"])]
            async (string url, IFileService fileService) =>
            {
                await using var stream = await fileService.DownloadFileAsync(url);

                return TypedResults.File(stream, MediaTypeNames.Image.Jpeg);
            }).Produces<Stream>(contentType: MediaTypeNames.Image.Jpeg);
    }
}
