using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicroCommerce.InventoryService.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using RedLockNet;
using MicroCommerce.BuildingBlocks.Common;
using Ardalis.GuardClauses;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using MicroCommerce.BuildingBlocks.EFCore;

namespace MicroCommerce.InventoryService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddEfCore<ApplicationDbContext>("db");
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

            return RedLockFactory.Create(
            [
                ConnectionMultiplexer.Connect(connectionString)
            ], sp.GetRequiredService<ILoggerFactory>());
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
}
