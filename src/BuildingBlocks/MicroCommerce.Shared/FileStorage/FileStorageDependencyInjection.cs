using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Shared.FileStorage
{
    public static class FileStorageDependencyInjection
    {
        public static void AddFileStorage(this IServiceCollection services, string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                rootPath = Directory.GetCurrentDirectory();
            }
            services.AddTransient<IStorageService>(_ => new FileStorageService(rootPath));
        }
    }
}
