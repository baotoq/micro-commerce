using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MicroCommerce.ApiService.Features.Profiles.Infrastructure;

public interface IAvatarImageService
{
    Task<string> ProcessAndUploadAvatarAsync(Stream imageStream, string originalFileName, CancellationToken ct = default);
    Task DeleteAvatarAsync(string avatarUrl, CancellationToken ct = default);
}

public class AvatarImageService : IAvatarImageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "avatars";
    private const int AvatarSize = 400;

    public AvatarImageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> ProcessAndUploadAvatarAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(imageStream, ct);

        // Crop to square from center
        var minDimension = Math.Min(image.Width, image.Height);
        var offsetX = (image.Width - minDimension) / 2;
        var offsetY = (image.Height - minDimension) / 2;

        image.Mutate(ctx => ctx
            .Crop(new Rectangle(offsetX, offsetY, minDimension, minDimension))
            .Resize(AvatarSize, AvatarSize));

        // Save as JPEG to memory stream
        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, ct);
        outputStream.Position = 0;

        // Upload to blob storage
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: ct);

        var blobName = $"{Guid.NewGuid()}.jpg";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            outputStream,
            new BlobHttpHeaders { ContentType = "image/jpeg" },
            cancellationToken: ct);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAvatarAsync(string avatarUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            return;

        try
        {
            var uri = new Uri(avatarUrl);
            var blobName = Path.GetFileName(uri.LocalPath);

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (Exception)
        {
            // Ignore deletion failures - blob may already be deleted or URL may be invalid
        }
    }
}
