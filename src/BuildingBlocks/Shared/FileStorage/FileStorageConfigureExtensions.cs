using Microsoft.Extensions.DependencyInjection;

namespace Shared.FileStorage
{
    public static class FileStorageConfigureExtensions
    {
        public static void AddFileStorage(this IServiceCollection services, string rootPath)
        {
            services.AddTransient<IStorageService>(resolver => new FileStorageService(rootPath));
        }
    }
}
