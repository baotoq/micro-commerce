using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UploadProductImage;

public sealed class UploadProductImageCommandHandler
    : IRequestHandler<UploadProductImageCommand, string>
{
    private readonly IImageUploadService _imageUploadService;

    public UploadProductImageCommandHandler(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    public async Task<string> Handle(
        UploadProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var imageUrl = await _imageUploadService.UploadImageAsync(
            request.ImageStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        return imageUrl;
    }
}

