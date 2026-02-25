using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.ApiService.Tests.Integration.Fixtures;

/// <summary>
/// Base class for integration tests that need per-test-class database isolation.
/// Provides helpers for creating authenticated/guest HTTP clients and direct DbContext access.
///
/// Usage: Subclass this, apply [Collection("Integration Tests")] on your test class,
/// override InitializeAsync to call ResetDatabase with the DbContext types you need.
/// </summary>
[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly ApiWebApplicationFactory Factory;

    protected IntegrationTestBase(ApiWebApplicationFactory factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Creates an authenticated HttpClient that sets the X-Test-UserId header.
    /// The FakeAuthenticationHandler injects this as ClaimTypes.NameIdentifier and "sub" claims.
    /// Use for endpoints that require authentication (RequireAuthorization).
    /// </summary>
    protected HttpClient CreateAuthenticatedClient(Guid userId)
    {
        HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        return client;
    }

    /// <summary>
    /// Creates an unauthenticated guest HttpClient.
    /// Optionally sets the buyer_id cookie for cart operations.
    /// </summary>
    protected HttpClient CreateGuestClient(Guid? buyerId = null)
    {
        HttpClient client = Factory.CreateClient();
        if (buyerId.HasValue)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"buyer_id={buyerId.Value}");
        }
        return client;
    }

    /// <summary>
    /// Creates a DI service scope for direct service/DbContext access.
    /// Caller is responsible for disposing the scope.
    /// </summary>
    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    /// <summary>
    /// Drops and recreates the schema for the given DbContext types.
    /// Call this in InitializeAsync to get a clean database state for each test class.
    ///
    /// Uses DROP SCHEMA ... CASCADE + MigrateAsync per context, so ONLY the specified schemas
    /// are affected. Other schemas (and their data) remain intact.
    ///
    /// This is safe to use in tests sharing a single PostgreSQL database with multiple schemas.
    /// </summary>
    protected async Task ResetDatabase(params Type[] dbContextTypes)
    {
        using IServiceScope scope = CreateScope();
        foreach (Type dbContextType in dbContextTypes)
        {
            DbContext db = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);

            // Get the schema name from the EF model's default schema annotation
            string? schemaName = db.Model.GetDefaultSchema();
            if (!string.IsNullOrEmpty(schemaName))
            {
                // Drop the schema and all its objects, then recreate via migrations.
                // Schema name comes from EF model metadata (not user input), so SQL injection is not a risk.
                // EF1002 is suppressed because parameterization is not supported for DDL schema names.
#pragma warning disable EF1002
                await db.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE");
#pragma warning restore EF1002
            }

            // Re-apply migrations to recreate the schema from scratch
            await db.Database.MigrateAsync();
        }
    }

    /// <summary>
    /// Override in subclass to reset database schemas before each test class runs.
    /// Example: await ResetDatabase(typeof(CatalogDbContext), typeof(InventoryDbContext));
    /// </summary>
    public virtual Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Override in subclass for test class teardown if needed.
    /// </summary>
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
