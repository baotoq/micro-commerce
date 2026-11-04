using System.Security.Cryptography;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.ApiService.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Testcontainers.PostgreSql;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// Runs the API in process against real PostgreSQL and Temporal containers, in the Development environment so
/// the OpenAPI document is served. FastEndpoints caches one app per fixture type, so the container
/// starts once per test run, and Testcontainers removes it afterwards.
/// JWT validation trusts <see cref="TestIdentity.SigningKey"/> instead of Keycloak; everything after
/// token validation runs for real. Integration events go to <see cref="Publisher"/>, a substitute,
/// instead of a Dapr sidecar.
/// </summary>
public class ApiFixture : AppFixture<Program>
{
    // SetupAsync runs once per test class, concurrently, against the one cached app; migrating in parallel races.
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);

    private PostgreSqlContainer _postgres = null!;
    private IContainer _temporal = null!;

    // Static: FastEndpoints makes a fixture instance per test class but caches one app.
    private static readonly string AppApiToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    /// <summary>What the sidecar sends as <c>dapr-api-token</c>; random per test run.</summary>
    public string DaprApiToken => AppApiToken;

    private static readonly IIntegrationEventPublisher SharedPublisher = Substitute.For<IIntegrationEventPublisher>();

    /// <summary>Receives the integration events the app publishes; shared by every test class, like the app.</summary>
    public IIntegrationEventPublisher Publisher => SharedPublisher;

    protected override async ValueTask PreSetupAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        _temporal = new ContainerBuilder("temporalio/temporal:1.5.1")
            .WithCommand(["server", "start-dev", "--ip", "0.0.0.0"])
            .WithPortBinding(7233, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(7233))
            .Build();

        await Task.WhenAll(_postgres.StartAsync(), _temporal.StartAsync());
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseEnvironment("Development");
        a.UseSetting("ConnectionStrings:micro-commerce-db", _postgres.GetConnectionString());
        a.UseSetting("Temporal:Address", $"127.0.0.1:{_temporal.GetMappedPublicPort(7233)}");
        a.UseSetting("APP_API_TOKEN", DaprApiToken);
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
        s.AddSingleton(Publisher);

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
