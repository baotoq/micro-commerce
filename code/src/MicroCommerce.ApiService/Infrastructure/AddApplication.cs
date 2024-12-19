using MicroCommerce.ApiService.Infrastructure.Interceptors;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Identity;

namespace MicroCommerce.ApiService.Infrastructure;

public static class AddApplicationDependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
        builder.Services.AddTransient<ICacheService, CacheService>();
        builder.Services.AddTransient<IFileService, FileService>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = false;
        });
    }
}
