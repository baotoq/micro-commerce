using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.IntegrationTests;

/// <summary>
/// Starts the whole AppHost once for all tests in the assembly, with throwaway containers and random
/// ports (<c>--EphemeralResources=true</c>). The seeded passwords and the Dapr app token are random
/// per run, so nothing secret is committed.
/// </summary>
public class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);

    private string _originalWorkingDirectory = null!;

    public DistributedApplication App { get; private set; } = null!;

    public string BuyerPassword { get; } = NewSecret();

    public string OperatorPassword { get; } = NewSecret();

    public string MerchantPassword { get; } = NewSecret();

    public HttpClient ApiClient { get; private set; } = null!;

    public HttpClient KeycloakClient { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        using var cts = new CancellationTokenSource(StartupTimeout);
        _originalWorkingDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetFullPath("../../../../../src/MicroCommerce.AppHost", AppContext.BaseDirectory));

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MicroCommerce_AppHost>(
            [
                "--EphemeralResources=true",
                $"--Parameters:dev-buyer-password={BuyerPassword}",
                $"--Parameters:dev-operator-password={OperatorPassword}",
                $"--Parameters:dev-merchant-password={MerchantPassword}",
                $"--Parameters:dapr-app-api-token={NewSecret()}",
            ],
            cts.Token);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Aspire.", LogLevel.Warning);
        });
        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

        App = await builder.BuildAsync(cts.Token);
        await App.StartAsync(cts.Token);
        await Task.WhenAll(
            App.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cts.Token),
            App.ResourceNotifications.WaitForResourceHealthyAsync("keycloak", cts.Token),
            App.ResourceNotifications.WaitForResourceHealthyAsync("temporal", cts.Token));

        ApiClient = App.CreateHttpClient("apiservice", "http");
        KeycloakClient = App.CreateHttpClient("keycloak", "http");
    }

    public async ValueTask DisposeAsync()
    {
        ApiClient?.Dispose();
        KeycloakClient?.Dispose();
        try
        {
            if (App is not null) await App.DisposeAsync();
        }
        finally
        {
            if (_originalWorkingDirectory is not null) Directory.SetCurrentDirectory(_originalWorkingDirectory);
        }
    }

    private static string NewSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
