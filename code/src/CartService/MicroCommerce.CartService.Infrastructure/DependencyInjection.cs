using Ardalis.GuardClauses;
using Elastic.Clients.Elasticsearch;
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
}
