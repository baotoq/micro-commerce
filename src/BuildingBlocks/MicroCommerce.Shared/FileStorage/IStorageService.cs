using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCommerce.Shared.FileStorage
{
    public interface IStorageService
    {
        Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
        Task SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    }
}
