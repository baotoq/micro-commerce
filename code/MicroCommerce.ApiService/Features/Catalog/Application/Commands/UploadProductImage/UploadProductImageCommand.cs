using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UploadProductImage;

public sealed record UploadProductImageCommand(
    Stream ImageStream,
    string FileName,
    string ContentType,
    long FileSize) : IRequest<string>;

