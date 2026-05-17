using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MicroCommerce.Catalog.FunctionalTests;

public class CatalogWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:catalogdb", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:cache", "localhost:6379");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOutputCacheStore>();
            services.AddSingleton<IOutputCacheStore, NoOpOutputCacheStore>();
        });
    }

    public async ValueTask InitializeAsync() => await _postgres.StartAsync();

    public new async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

internal sealed class NoOpOutputCacheStore : IOutputCacheStore
{
    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken) => ValueTask.CompletedTask;
    public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) => ValueTask.FromResult<byte[]?>(null);
    public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
