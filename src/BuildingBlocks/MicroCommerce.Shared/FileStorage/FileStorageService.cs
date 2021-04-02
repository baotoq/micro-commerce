using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCommerce.Shared.FileStorage
{
    public class FileStorageService : IStorageService
    {
        private readonly string _rootPath;

        public FileStorageService(string rootPath)
        {
            _rootPath = rootPath;
        }

        public async Task SaveAsync(Stream stream, string path, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, path);
            await using var output = File.Create(filePath);
            await stream.CopyToAsync(output, cancellationToken);
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, path);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
