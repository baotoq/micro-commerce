using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MicroCommerce.ApiService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MicroCommerce.FunctionalTests;

/// <summary>
/// The database is unreachable when the API starts (spec edge case, FR-004): the API stays up and
/// reports unhealthy, then applies migrations and reports healthy once the database appears, with no
/// restart. Uses its own app and containers, so no other test's worker or data is involved.
/// </summary>
public class DatabaseStartupTests : IAsyncLifetime
{
    private readonly int _port = FreePort();
    private readonly string _password = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
    private PostgreSqlContainer? _postgres;
    private IContainer _temporal = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public async ValueTask InitializeAsync()
    {
        // Its own Temporal, so its worker never picks up workflows meant for the shared test app.
        _temporal = new ContainerBuilder("temporalio/temporal:1.5.1")
            .WithCommand(["server", "start-dev", "--ip", "0.0.0.0"])
            .WithPortBinding(7233, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(7233))
            .Build();
        await _temporal.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Development");
            b.UseSetting("ConnectionStrings:micro-commerce-db",
                $"Host=127.0.0.1;Port={_port};Database=micro-commerce-db;Username=postgres;Password={_password};Timeout=2");
            b.UseSetting("Temporal:Address", $"127.0.0.1:{_temporal.GetMappedPublicPort(7233)}");
            // Let the migrator's own loop do the retrying, so recovery is quick.
            b.UseSetting("Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableRetry", "true");
        });
    }

    [Fact]
    public async Task Api_reports_unhealthy_until_the_database_is_reachable_then_recovers()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        (await client.GetAsync("/alive", ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync("/health", ct)).StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);

        _postgres = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("micro-commerce-db")
            .WithPassword(_password)
            .WithPortBinding(_port, 5432)
            .Build();
        await _postgres.StartAsync(ct);

        var deadline = DateTime.UtcNow.AddSeconds(60);
        while ((await client.GetAsync("/health", ct)).StatusCode != HttpStatusCode.OK)
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("The API didn't become healthy after the database started.");
            await Task.Delay(500, ct);
        }

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Database.GetPendingMigrationsAsync(ct)).ShouldBeEmpty();
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        if (_postgres is not null) await _postgres.DisposeAsync();
        await _temporal.DisposeAsync();
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
