using MicroCommerce.ApiService.Data;
using MicroCommerce.ApiService.Features.Accounts.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// Runs the API in-process against real Postgres and Redis containers.
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
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<AccountsDbContext>().Database.MigrateAsync();
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
