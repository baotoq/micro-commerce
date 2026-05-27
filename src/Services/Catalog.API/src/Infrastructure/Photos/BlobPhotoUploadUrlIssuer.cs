using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MicroCommerce.Catalog.Application.Products.Photos;

namespace MicroCommerce.Catalog.Infrastructure.Photos;

/// <summary>
/// Generates write-only SAS URLs for product photo uploads against the "photos" container.
/// SAS TTL is 15 minutes per PRD §4. Uses BlobSasBuilder against the user-delegated /
/// shared-key BlobClient (Azurite emulator works with shared-key in dev).
/// </summary>
internal sealed class BlobPhotoUploadUrlIssuer(BlobServiceClient serviceClient) : IPhotoUploadUrlIssuer
{
    private const string ContainerName = "photos";
    private static readonly TimeSpan SasTtl = TimeSpan.FromMinutes(15);

    public async Task<PhotoUploadUrlResponse> IssueAsync(string contentType, CancellationToken ct)
    {
        var container = serviceClient.GetBlobContainerClient(ContainerName);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobName = $"products/{Guid.NewGuid():N}{ExtensionFor(contentType)}";
        var blobClient = container.GetBlobClient(blobName);

        var expiresAt = DateTimeOffset.UtcNow.Add(SasTtl);
        var sas = new BlobSasBuilder
        {
            BlobContainerName = ContainerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = expiresAt,
            ContentType = contentType,
        };
        sas.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Blob client cannot generate SAS URIs. Verify the client was constructed with shared-key credentials.");
        }
        var uploadUri = blobClient.GenerateSasUri(sas);

        return new PhotoUploadUrlResponse(
            UploadUrl: uploadUri.ToString(),
            BlobUrl: blobClient.Uri.ToString(),
            ExpiresAt: expiresAt);
    }

    private static string ExtensionFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/heic" => ".heic",
        _ => string.Empty,
    };
}
