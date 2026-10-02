using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.IntegrationTests;

/// <summary>
/// Starts the whole AppHost (API + Postgres + Redis + Keycloak containers) once for all tests in the assembly.
/// </summary>
public class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    private DistributedApplication _app = null!;

    public HttpClient ApiClient { get; private set; } = null!;

    public HttpClient KeycloakClient { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        using var cts = new CancellationTokenSource(StartupTimeout);

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MicroCommerce_AppHost>(
            ["--EphemeralResources=true"], cts.Token);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Aspire.", LogLevel.Warning);
        });
        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

        _app = await builder.BuildAsync(cts.Token);
        await _app.StartAsync(cts.Token);
        await Task.WhenAll(
            _app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cts.Token),
            _app.ResourceNotifications.WaitForResourceHealthyAsync("keycloak", cts.Token));

        ApiClient = _app.CreateHttpClient("apiservice", "http");
        KeycloakClient = _app.CreateHttpClient("keycloak", "http");
    }

    public async ValueTask DisposeAsync()
    {
        ApiClient.Dispose();
        KeycloakClient.Dispose();
        await _app.DisposeAsync();
    }
}
