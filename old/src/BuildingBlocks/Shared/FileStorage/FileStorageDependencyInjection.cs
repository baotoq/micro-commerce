using Microsoft.Extensions.DependencyInjection;

namespace Shared.FileStorage
{
    public static class FileStorageDependencyInjection
    {
        public static void AddFileStorage(this IServiceCollection services, string rootPath)
        {
            services.AddTransient<IStorageService>(resolver => new FileStorageService(rootPath));
        }
    }
}
