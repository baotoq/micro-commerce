using Ardalis.GuardClauses;
using Elastic.Clients.Elasticsearch;
using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Infrastructure.Data;
using MicroCommerce.CartService.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace MicroCommerce.CartService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatorDomainEventDispatcher();
        builder.AddEfCore("db");
        builder.AddElasticsearch("elasticsearch");
        builder.AddRedisDistributedCache("redis");
        builder.AddRedisOutputCache("redis");
        builder.AddRedLock("redis");
        builder.AddAzureBlobClient("blobs");
    }

    private static void AddRedLock(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddSingleton<IDistributedLockFactory>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(connectionName);
            Guard.Against.NullOrEmpty(connectionString, message: "Redis connection string is required.");

            return RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect(connectionString)
            }, sp.GetRequiredService<ILoggerFactory>());
        });
    }

    private static void AddElasticsearch(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.AddElasticsearchClient(connectionName, null, client =>
        {
            // client.DefaultMappingFor<ProductDocument>(i => i
            //         .IndexName(ProductDocument.IndexPattern)
            //         .IdProperty(p => p.Id)
            //     )
            //     .EnableDebugMode();
        });
    }

    private static void AddEfCore(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddScoped<DispatchDomainEventsInterceptor>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(connectionName);
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>(s =>
        {
            s.DisableRetry = true;
        });
    }
}
