using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Features.Catalog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroCommerce.ApiService.Infrastructure.Data;

/// <summary>Whether this process has brought the database schema up to date.</summary>
public sealed class DatabaseMigrationStatus
{
    private readonly TaskCompletionSource _applied = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Applied => _applied.Task;

    public void MarkApplied() => _applied.TrySetResult();
}

/// <summary>
/// Development only: applies every context's migrations, then seeds the starter Category tree, in the background and keeps retrying while the database is
/// unreachable, so the API stays up and reports unhealthy instead of exiting (spec edge case, FR-004).
/// </summary>
public sealed class DatabaseMigrator(
    IServiceScopeFactory scopes,
    DatabaseMigrationStatus status,
    ILogger<DatabaseMigrator> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var services = scope.ServiceProvider;
                await services.GetRequiredService<AppDbContext>().Database.MigrateAsync(stoppingToken);
                await services.GetRequiredService<AccountsDbContext>().Database.MigrateAsync(stoppingToken);
                var catalog = services.GetRequiredService<CatalogDbContext>();
                await catalog.Database.MigrateAsync(stoppingToken);

                // Seeded before MarkApplied, so nothing that waits for it can race the seed (002 research R4).
                await CatalogSeeder.SeedAsync(catalog, stoppingToken);
                status.MarkApplied();
                logger.LogInformation("Database migrations applied");
                return;
            }
            catch (Exception e) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(e, "Applying database migrations failed; retrying in {RetryDelay}", RetryDelay);
            }

            await Task.Delay(RetryDelay, stoppingToken);
        }
    }
}

/// <summary>Readiness: unhealthy until <see cref="DatabaseMigrator"/> has applied the migrations.</summary>
public sealed class DatabaseMigrationHealthCheck(DatabaseMigrationStatus status) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(status.Applied.IsCompletedSuccessfully
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database migrations haven't been applied yet."));
}
