using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.FileStorage
{
    public interface IStorageService
    {
        Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
        Task SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    }
}
