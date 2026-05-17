using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.IntegrationTests;

public class ApiFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MicroCommerce_AppHost>();

        appHost.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        appHost.Services.ConfigureHttpClientDefaults(c => c.AddStandardResilienceHandler());

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("server")
            .WaitAsync(TimeSpan.FromSeconds(120));

        HttpClient = _app.CreateHttpClient("server");
    }

    public async Task DisposeAsync()
    {
        HttpClient.Dispose();
        if (_app != null)
            await _app.DisposeAsync();
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    }
}
