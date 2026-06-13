using System.Net.Http.Headers;
using System.Text;
using Dapr.Client;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MicroCommerce.Catalog.FunctionalTests.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.Azurite;
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

    private readonly AzuriteContainer _azurite = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
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
        // Storefront checkout tests price lines from seeded products (MC-VS-001 $86) and validate
        // the seeded WELCOME10 promo, so the test API must run its startup seeders. The seeders are
        // idempotent (insert-if-absent), so existing functional tests — which tolerate seed data —
        // are unaffected.
        builder.UseSetting("SEED_PRODUCTS", "true");
        builder.UseSetting("ConnectionStrings:catalogdb", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:cache", "localhost:6379");
        builder.UseSetting("ConnectionStrings:photos", _azurite.GetConnectionString());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOutputCacheStore>();
            services.AddSingleton<SpyOutputCacheStore>();
            services.AddSingleton<IOutputCacheStore>(sp => sp.GetRequiredService<SpyOutputCacheStore>());

            services.RemoveAll<DaprClient>();
            var grpcEndpoint = $"http://localhost:{_dapr.GetMappedPublicPort(50001)}";
            services.AddSingleton(new DaprClientBuilder().UseGrpcEndpoint(grpcEndpoint).Build());

            // Re-point the REAL JwtBearer handler at self-minted tokens (TestTokens) so the full
            // authentication + seller-policy pipeline runs without a Keycloak container or any
            // back-channel metadata fetch. Pre-seeding Configuration short-circuits the
            // ConfigurationManager that AddKeycloakJwtBearer wires via service discovery.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.Authority = null;
                options.MetadataAddress = null!;
                options.Configuration = new OpenIdConnectConfiguration { Issuer = TestTokens.Issuer };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestTokens.Issuer,
                    ValidateAudience = true,
                    ValidAudience = TestTokens.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TestTokens.SigningKey,
                    ValidateLifetime = true,
                    RoleClaimType = TestTokens.RoleClaimType,
                };
            });
        });
    }

    /// <summary>
    /// HttpClient whose default Authorization header carries a self-minted seller token, so calls
    /// pass the real JwtBearer handler and the seller policy. Use for authenticated write requests.
    /// </summary>
    public HttpClient CreateSellerClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokens.Mint(["seller"]));
        return client;
    }

    /// <summary>HttpClient with a self-minted role-less buyer token (BuyerPolicy = authenticated).</summary>
    public HttpClient CreateBuyerClient(string email = "buyer-tests@microcommerce.dev")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokens.Mint([], email: email));
        return client;
    }

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _dapr.StartAsync(), _azurite.StartAsync());
    }

    public new async ValueTask DisposeAsync()
    {
        await _dapr.DisposeAsync();
        await _postgres.DisposeAsync();
        await _azurite.DisposeAsync();
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
