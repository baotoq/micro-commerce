using MicroCommerce.ApiService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// Runs the API in-process against real Postgres and Redis containers.
/// FastEndpoints caches one app instance per fixture type, so the containers start once per test run.
/// Testcontainers removes them when the run ends.
/// </summary>
public class ApiFixture : AppFixture<Program>
{
    private PostgreSqlContainer _postgres = null!;
    private RedisContainer _redis = null!;

    protected override async ValueTask PreSetupAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        _redis = new RedisBuilder("redis:7-alpine").Build();

        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseSetting("ConnectionStrings:micro-commerce-db", _postgres.GetConnectionString());
        a.UseSetting("ConnectionStrings:redis", _redis.GetConnectionString());
    }

    protected override async ValueTask SetupAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }
}
