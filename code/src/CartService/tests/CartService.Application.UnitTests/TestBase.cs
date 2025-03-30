using MicroCommerce.CartService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CartService.Application.UnitTests;

public abstract class TestBase : IAsyncLifetime
{
    protected ApplicationDbContext SeedContext { get; private set; } = null!;
    protected ApplicationDbContext VerifyContext { get; private set; } = null!;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        SeedContext = new ApplicationDbContext(contextOptions);
        await SeedContext.Database.EnsureCreatedAsync();

        VerifyContext = new ApplicationDbContext(contextOptions);
        VerifyContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }
}
