using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Features.Products;

public class UploadProductImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/products/images", async ([FromForm] IFormFile file, IFileService fileService) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await fileService.UploadFileAsync(Guid.CreateVersion7() + Path.GetExtension(file.FileName), stream);

            return TypedResults.Ok(new
            {
                uri = result
            });
        }).ProducesValidationProblem().DisableAntiforgery();
    }
}
