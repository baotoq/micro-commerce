using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Ordering.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// Runs the API in-process against real Postgres, Redis and Temporal containers.
/// FastEndpoints caches one app instance per fixture type, so the containers start once per test run.
/// Testcontainers removes them when the run ends.
/// JWT validation trusts <see cref="TestIdentity.SigningKey"/> instead of Keycloak; everything after
/// token validation runs for real.
/// </summary>
public class ApiFixture : AppFixture<Program>
{
    // SetupAsync runs once per test class, concurrently, against the one cached app; migrating in parallel races.
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);

    private PostgreSqlContainer _postgres = null!;
    private RedisContainer _redis = null!;
    private IContainer _temporal = null!;

    protected override async ValueTask PreSetupAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        _redis = new RedisBuilder("redis:7-alpine").Build();
        _temporal = new ContainerBuilder("temporalio/temporal:1.5.1")
            .WithCommand(["server", "start-dev", "--ip", "0.0.0.0"])
            .WithPortBinding(7233, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(7233))
            .Build();

        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _temporal.StartAsync());
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseSetting("ConnectionStrings:micro-commerce-db", _postgres.GetConnectionString());
        a.UseSetting("ConnectionStrings:redis", _redis.GetConnectionString());
        a.UseSetting("Temporal:Address", $"127.0.0.1:{_temporal.GetMappedPublicPort(7233)}");
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
        s.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, o =>
        {
            // A static configuration stops JwtBearer from fetching Keycloak's discovery document.
            o.Authority = null;
            o.MetadataAddress = null!;
            o.ConfigurationManager = null;
            o.Configuration = new OpenIdConnectConfiguration { Issuer = TestIdentity.Issuer };
            o.TokenValidationParameters.ValidIssuer = TestIdentity.Issuer;
            o.TokenValidationParameters.ValidAudience = TestIdentity.Audience;
            o.TokenValidationParameters.IssuerSigningKey = TestIdentity.SigningKey;
        });
    }

    protected override async ValueTask SetupAsync()
    {
        await MigrationLock.WaitAsync();
        try
        {
            await using var scope = Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<AccountsDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.MigrateAsync();
        }
        finally
        {
            MigrationLock.Release();
        }
    }

    /// <summary>A client whose requests carry a valid token for <paramref name="actor"/>.</summary>
    public HttpClient ClientFor(TestActor actor) => ClientFor(TestIdentity.TokenFor(actor));

    /// <summary>A client whose requests carry <paramref name="token"/> as their bearer token.</summary>
    public HttpClient ClientFor(string token) => CreateClient(c => c.Authenticate(token));
}

public class DevelopmentApiFixture : ApiFixture
{
    protected override void ConfigureApp(IWebHostBuilder a)
    {
        base.ConfigureApp(a);
        a.UseEnvironment("Development");
    }
}
