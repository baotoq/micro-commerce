using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Shared.FileStorage
{
    public static class FileStorageDependencyInjection
    {
        public static void AddFileStorage(this IServiceCollection services, string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentNullException(nameof(rootPath));
            }
            services.AddTransient<IStorageService>(_ => new FileStorageService(rootPath));
        }
    }
}
