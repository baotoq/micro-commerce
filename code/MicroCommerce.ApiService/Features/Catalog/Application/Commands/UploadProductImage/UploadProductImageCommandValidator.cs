using FluentValidation;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UploadProductImage;

public sealed class UploadProductImageCommandValidator
    : AbstractValidator<UploadProductImageCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    ];

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(BeAllowedContentType)
            .WithMessage($"File must be an image. Allowed types: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File cannot be empty.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"File size cannot exceed {MaxFileSize / 1024 / 1024}MB.");
    }

    private static bool BeAllowedContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
}

