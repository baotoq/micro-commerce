using System.Text;
using Dapr.Client;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MicroCommerce.Catalog.FunctionalTests;

public class CatalogWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PubSubYaml = """
        apiVersion: dapr.io/v1alpha1
        kind: Component
        metadata:
          name: pubsub
        spec:
          type: pubsub.in-memory
          version: v1
        """;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private readonly IContainer _dapr = new ContainerBuilder()
        .WithImage("daprio/daprd:1.15.0")
        .WithResourceMapping(Encoding.UTF8.GetBytes(PubSubYaml), "/components/pubsub.yaml")
        .WithCommand(
            "./daprd",
            "--app-id", "catalog-api",
            "--dapr-listen-addresses", "0.0.0.0",
            "--resources-path", "/components")
        .WithPortBinding(50001, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("dapr initialized"))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:catalogdb", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:cache", "localhost:6379");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOutputCacheStore>();
            services.AddSingleton<SpyOutputCacheStore>();
            services.AddSingleton<IOutputCacheStore>(sp => sp.GetRequiredService<SpyOutputCacheStore>());

            services.RemoveAll<DaprClient>();
            var grpcEndpoint = $"http://localhost:{_dapr.GetMappedPublicPort(50001)}";
            services.AddSingleton(new DaprClientBuilder().UseGrpcEndpoint(grpcEndpoint).Build());
        });
    }

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _dapr.StartAsync());
    }

    public new async ValueTask DisposeAsync()
    {
        await _dapr.DisposeAsync();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

internal sealed class SpyOutputCacheStore : IOutputCacheStore
{
    private readonly List<string> _evictedTags = [];

    public IReadOnlyList<string> EvictedTags
    {
        get { lock (_evictedTags) return _evictedTags.ToArray(); }
    }

    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        lock (_evictedTags) _evictedTags.Add(tag);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) => ValueTask.FromResult<byte[]?>(null);
    public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
