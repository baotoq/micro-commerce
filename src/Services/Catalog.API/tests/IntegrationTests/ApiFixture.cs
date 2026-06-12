using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.IntegrationTests;

public class ApiFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public HttpClient HttpClient { get; private set; } = null!;

    /// <summary>
    /// HttpClient bound to the Keycloak resource's HTTP endpoint, resolved from the app
    /// model the same way <see cref="HttpClient"/> resolves catalog-api. Use
    /// <c>KeycloakHttpClient.BaseAddress</c> as the issuer/token-endpoint base URL.
    /// </summary>
    public HttpClient KeycloakHttpClient { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MicroCommerce_AppHost>();

        appHost.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        appHost.Services.ConfigureHttpClientDefaults(c => c.AddStandardResilienceHandler());

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Keycloak is declared OUTSIDE the Testing guard in AppHost.cs, so it is part of the
        // Testing-env app graph. catalog-api WaitFor(keycloak)s, but wait on keycloak explicitly
        // too so the token endpoint is reachable before the auth integration test runs.
        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("keycloak")
            .WaitAsync(TimeSpan.FromSeconds(180));

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("catalog-api")
            .WaitAsync(TimeSpan.FromSeconds(120));

        HttpClient = _app.CreateHttpClient("catalog-api");
        KeycloakHttpClient = _app.CreateHttpClient("keycloak");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        HttpClient.Dispose();
        KeycloakHttpClient.Dispose();
        if (_app != null)
            await _app.DisposeAsync();
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    }
}
