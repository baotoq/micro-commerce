using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.FileStorage
{
    public class FileStorageService : IStorageService
    {
        private readonly string _rootPath;

        public FileStorageService(string rootPath)
        {
            _rootPath = rootPath;
        }

        public async Task SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, fileName);
            await using var output = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(output, cancellationToken);
        }

        public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath), cancellationToken);
            }
        }
    }
}
