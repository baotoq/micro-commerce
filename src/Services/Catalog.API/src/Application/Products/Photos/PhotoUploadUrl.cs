using MediatR;

namespace MicroCommerce.Catalog.Application.Products.Photos;

public record PhotoUploadUrlCommand(string ContentType, long SizeBytes) : IRequest<PhotoUploadUrlResponse>;

public record PhotoUploadUrlResponse(string UploadUrl, string BlobUrl, DateTimeOffset ExpiresAt);

/// <summary>
/// Defines the contract for issuing a write-only SAS URL for a single product photo blob.
/// Concrete implementation lives in Infrastructure (depends on Azure SDK).
/// </summary>
public interface IPhotoUploadUrlIssuer
{
    /// <summary>
    /// Allocates a blob name under products/, issues a write+create SAS valid for ~15 minutes,
    /// and returns both the signed upload URL and the bare blob URL (without SAS) for storage.
    /// </summary>
    Task<PhotoUploadUrlResponse> IssueAsync(string contentType, CancellationToken ct);
}

internal sealed class PhotoUploadUrlHandler(IPhotoUploadUrlIssuer issuer) : IRequestHandler<PhotoUploadUrlCommand, PhotoUploadUrlResponse>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/heic",
    };
    private const long MaxBytes = 8L * 1024 * 1024;

    public async Task<PhotoUploadUrlResponse> Handle(PhotoUploadUrlCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ContentType) || !AllowedContentTypes.Contains(request.ContentType))
            throw new ArgumentException($"Content type '{request.ContentType}' is not allowed.", nameof(request.ContentType));

        if (request.SizeBytes <= 0 || request.SizeBytes > MaxBytes)
            throw new ArgumentOutOfRangeException(nameof(request.SizeBytes), $"sizeBytes must be between 1 and {MaxBytes}.");

        return await issuer.IssueAsync(request.ContentType, ct);
    }
}
