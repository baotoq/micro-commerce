using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.BuildingBlocks.EFCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.BuildingBlocks.EFCore;

public static class DependencyInjection
{
    public static void AddEfCore<TContext>(this IHostApplicationBuilder builder, string connectionName) where TContext : DbContext
    {
        builder.Services.AddMediatorDomainEventDispatcher();
        builder.Services.AddScoped<DispatchDomainEventsInterceptor>();
        builder.Services.AddDbContext<TContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(connectionName);
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });
        builder.EnrichNpgsqlDbContext<TContext>(s =>
        {
            s.DisableRetry = true;
        });
    }
}
