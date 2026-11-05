using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.E2ETests;

/// <summary>
/// Starts the whole AppHost once for the run, with throwaway containers and random passwords, and
/// one Chromium browser (research R2–R4). The web app is pinned to port 5174, the dev realm's second
/// redirect URI, so the suite can run next to a developer's own Marketplace on 5173 and 8080.
/// Copied from the integration tests' AppHostFixture rather than shared, because it differs in the
/// web port, the Keycloak admin password, the pre-flight checks and the resources it waits for.
/// </summary>
public class MarketplaceFixture : IAsyncLifetime
{
    public const int WebPort = 5174;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);
    private static readonly string[] Resources = ["apiservice", "keycloak", "temporal", "web"];

    private string? _originalWorkingDirectory;
    private IPlaywright? _playwright;

    public DistributedApplication App { get; private set; } = null!;

    public IBrowser Browser { get; private set; } = null!;

    public HttpClient ApiClient { get; private set; } = null!;

    public HttpClient KeycloakClient { get; private set; } = null!;

    /// <summary>The web app's address, <c>http://localhost:5174</c>.</summary>
    public string WebUrl { get; } = $"http://localhost:{WebPort}";

    /// <summary>The identity provider's origin as the browser sees it, for example <c>http://localhost:49152</c>.</summary>
    public string KeycloakUrl { get; private set; } = null!;

    public TestAccounts Accounts { get; private set; } = null!;

    // Random per run and never logged (FR-009). Passed to the AppHost as parameters.
    internal string BuyerPassword { get; } = NewSecret();

    internal string OperatorPassword { get; } = NewSecret();

    internal string MerchantPassword { get; } = NewSecret();

    internal string KeycloakAdminPassword { get; } = NewSecret();

    public async ValueTask InitializeAsync()
    {
        await PreflightAsync();

        using var cts = new CancellationTokenSource(StartupTimeout);
        _originalWorkingDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetFullPath("../../../../../src/MicroCommerce.AppHost", AppContext.BaseDirectory));

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MicroCommerce_AppHost>(
            [
                "--EphemeralResources=true",
                $"--WebPort={WebPort}",
                $"--Parameters:dev-buyer-password={BuyerPassword}",
                $"--Parameters:dev-operator-password={OperatorPassword}",
                $"--Parameters:dev-merchant-password={MerchantPassword}",
                $"--Parameters:dapr-app-api-token={NewSecret()}",
                $"--Parameters:keycloak-password={KeycloakAdminPassword}",
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

        // One wait per resource, so a timeout names the resource that never became healthy (FR-003).
        await Task.WhenAll(Resources.Select(name => WaitHealthyAsync(name, cts.Token)));

        var web = App.GetEndpoint("web", "http");
        web.Port.ShouldBe(WebPort, "The web app must be on the port the dev realm registers for the e2e suite.");
        KeycloakUrl = App.GetEndpoint("keycloak", "http").GetLeftPart(UriPartial.Authority);

        ApiClient = App.CreateHttpClient("apiservice", "http");
        KeycloakClient = App.CreateHttpClient("keycloak", "http");
        Accounts = new TestAccounts(this);

        // Installs Chromium into the per-user Playwright cache on first use; a no-op afterwards (research R4).
        var installed = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (installed != 0) throw new InvalidOperationException($"Chromium install failed (exit code {installed}).");

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = Environment.GetEnvironmentVariable("HEADED") != "1",
        });
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Browser is not null) await Browser.DisposeAsync();
            _playwright?.Dispose();
            ApiClient?.Dispose();
            KeycloakClient?.Dispose();
            if (App is not null) await App.DisposeAsync();
        }
        finally
        {
            if (_originalWorkingDirectory is not null) Directory.SetCurrentDirectory(_originalWorkingDirectory);
        }
    }

    private async Task WaitHealthyAsync(string name, CancellationToken ct)
    {
        try
        {
            await App.ResourceNotifications.WaitForResourceHealthyAsync(name, ct);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Resource '{name}' did not become healthy within {StartupTimeout.TotalMinutes:0} minutes.");
        }
    }

    /// <summary>Stops before anything starts, naming the missing prerequisite (FR-005).</summary>
    private static async Task PreflightAsync()
    {
        // Matches Aspire's own choice of container runtime.
        var runtime = string.Equals(Environment.GetEnvironmentVariable("ASPIRE_CONTAINER_RUNTIME"), "podman", StringComparison.OrdinalIgnoreCase)
            ? "podman"
            : "docker";

        if (!await SucceedsAsync(runtime, "info"))
            throw new InvalidOperationException($"Container runtime not running: start {runtime} and run again.");

        if (!await SucceedsAsync("dapr", "--version"))
            throw new InvalidOperationException("Dapr CLI not installed: run `dapr init --slim`.");

        if (PortInUse(WebPort))
            throw new InvalidOperationException($"Port {WebPort} is in use: the e2e suite serves the web app there. Free the port and run again.");
    }

    private static async Task<bool> SucceedsAsync(string fileName, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            });
            if (process is null) return false;

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            // Drain the pipes so a chatty command can't block on a full buffer.
            var drain = Task.WhenAll(process.StandardOutput.ReadToEndAsync(timeout.Token), process.StandardError.ReadToEndAsync(timeout.Token));
            await process.WaitForExitAsync(timeout.Token);
            await drain;
            return process.ExitCode == 0;
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or OperationCanceledException)
        {
            return false;
        }
    }

    private static bool PortInUse(int port)
    {
        // Vite may listen on IPv4 or IPv6 loopback, so check both.
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            try
            {
                using var client = new TcpClient(address.AddressFamily);
                if (client.ConnectAsync(address, port).Wait(TimeSpan.FromMilliseconds(500)) && client.Connected) return true;
            }
            catch (AggregateException)
            {
                // Connection refused: nothing listens on this address.
            }
        }

        return false;
    }

    private static string NewSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
