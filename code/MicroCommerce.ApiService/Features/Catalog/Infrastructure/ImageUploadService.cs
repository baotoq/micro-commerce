using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MicroCommerce.ApiService.Features.Catalog.Infrastructure;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public class ImageUploadService : IImageUploadService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "product-images";

    public ImageUploadService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        // Create container if it doesn't exist (with public access for images)
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken);

        // Generate unique blob name
        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        // Upload with content type
        await blobClient.UploadAsync(
            imageStream,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }
}

