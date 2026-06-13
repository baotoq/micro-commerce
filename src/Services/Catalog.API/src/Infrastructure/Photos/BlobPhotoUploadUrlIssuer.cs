using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MicroCommerce.Catalog.Application.Products.Photos;

namespace MicroCommerce.Catalog.Infrastructure.Photos;

/// <summary>
/// Generates write-only SAS URLs for product photo uploads against the "photos" container.
/// SAS TTL is 15 minutes per PRD §4. Uses BlobSasBuilder against the user-delegated /
/// shared-key BlobClient (Azurite emulator works with shared-key in dev).
/// </summary>
internal sealed class BlobPhotoUploadUrlIssuer(BlobServiceClient serviceClient, StorageSharedKeyCredential credential) : IPhotoUploadUrlIssuer
{
    private const string ContainerName = "photos";
    private static readonly TimeSpan SasTtl = TimeSpan.FromMinutes(15);

    public async Task<PhotoUploadUrlResponse> IssueAsync(string contentType, CancellationToken ct)
    {
        var container = serviceClient.GetBlobContainerClient(ContainerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blobName = $"products/{Guid.NewGuid():N}{ExtensionFor(contentType)}";
        // Build the blob URI manually: the SDK drops the container segment for custom-host
        // path-style Azurite endpoints. See PhotoUploadSas.BuildBlobUri.
        var blobUri = PhotoUploadSas.BuildBlobUri(serviceClient.Uri, ContainerName, blobName);

        var expiresAt = DateTimeOffset.UtcNow.Add(SasTtl);
        // TODO(security): SAS token does not enforce ContentLength on the actual PUT;
        // callers can upload arbitrary-sized payloads. Mitigations to consider:
        // (a) lifecycle policy to delete blobs over 8 MB, (b) server-side validation
        // post-upload, (c) shorter SAS TTL combined with metric-based alerting.
        var sas = new BlobSasBuilder
        {
            BlobContainerName = ContainerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = expiresAt,
            ContentType = contentType,
        };
        sas.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        // Sign with the shared key via PhotoUploadSas rather than blobClient.GenerateSasUri, whose
        // container-name validation fails for custom-host path-style Azurite endpoints. See PhotoUploadSas.
        var uploadUri = PhotoUploadSas.BuildUploadUri(blobUri, sas, credential);

        return new PhotoUploadUrlResponse(
            UploadUrl: uploadUri.ToString(),
            BlobUrl: blobUri.ToString(),
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
