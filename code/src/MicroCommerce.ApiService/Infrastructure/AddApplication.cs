using MicroCommerce.ApiService.Infrastructure.Interceptors;
using MicroCommerce.ApiService.Services;

namespace MicroCommerce.ApiService.Infrastructure;

public static class AddApplicationDependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
        builder.Services.AddTransient<ICacheService, CacheService>();
        builder.Services.AddTransient<IFileService, FileService>();
    }
}
