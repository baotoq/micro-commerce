using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.ProductImages;

/// <summary>Where Product image bytes live: one blob per image, named <c>{productId}/{imageId}</c> (research R7).</summary>
public interface IProductImageStore
{
    Task PutAsync(ProductId productId, ProductImageId imageId, Stream content, string contentType, CancellationToken ct);

    Task<(Stream Content, string ContentType)?> OpenReadAsync(ProductId productId, ProductImageId imageId, CancellationToken ct);

    /// <summary>Best effort: returns false instead of throwing when the blob couldn't be deleted.</summary>
    Task<bool> TryDeleteAsync(ProductId productId, ProductImageId imageId, CancellationToken ct);
}

public sealed class BlobProductImageStore(BlobServiceClient blobs) : IProductImageStore
{
    public const string ContainerName = "product-images";

    // Created once per process. The AppHost isn't there in functional tests, so the app makes it itself.
    private readonly Lazy<Task<BlobContainerClient>> _container = new(async () =>
    {
        var container = blobs.GetBlobContainerClient(ContainerName);
        await container.CreateIfNotExistsAsync();
        return container;
    });

    public async Task PutAsync(ProductId productId, ProductImageId imageId, Stream content, string contentType, CancellationToken ct)
    {
        var blob = (await _container.Value).GetBlobClient(BlobName(productId, imageId));
        await blob.UploadAsync(content, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } }, ct);
    }

    public async Task<(Stream Content, string ContentType)?> OpenReadAsync(ProductId productId, ProductImageId imageId, CancellationToken ct)
    {
        var blob = (await _container.Value).GetBlobClient(BlobName(productId, imageId));
        try
        {
            var download = await blob.DownloadStreamingAsync(cancellationToken: ct);
            return (download.Value.Content, download.Value.Details.ContentType);
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }

    public async Task<bool> TryDeleteAsync(ProductId productId, ProductImageId imageId, CancellationToken ct)
    {
        try
        {
            await (await _container.Value).GetBlobClient(BlobName(productId, imageId)).DeleteIfExistsAsync(cancellationToken: ct);
            return true;
        }
        catch (RequestFailedException)
        {
            return false;
        }
    }

    private static string BlobName(ProductId productId, ProductImageId imageId) => $"{productId.Value}/{imageId.Value}";
}
