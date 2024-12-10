using Azure.Storage.Blobs;

namespace MicroCommerce.ApiService.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(string fileName, Stream stream, CancellationToken cancellationToken = default);
    Task CreateContainerIfNotExistsAsync(CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileName, CancellationToken cancellationToken = default);
}

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "fileuploads";

    public FileService(BlobServiceClient blobServiceClient, ILogger<FileService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string fileName, Stream stream, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var response = await blobClient.UploadAsync(stream, overwrite: true, cancellationToken);

        if (response.GetRawResponse().IsError)
        {
            _logger.LogError("Failed to upload file {FileName} to blob storage {Info}", fileName, response.ToString());
            throw new Exception($"Failed to upload file {fileName}");
        }

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var memoryStream = new MemoryStream();
        var response = await blobClient.DownloadToAsync(memoryStream, cancellationToken);

        if (response.IsError)
        {
            _logger.LogError("Failed to download file {FileName} from blob storage {Info}", fileName, response.ToString());
            throw new Exception($"Failed to download file {fileName}");
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task CreateContainerIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }
}
