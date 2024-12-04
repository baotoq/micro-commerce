using System.Diagnostics;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace MicroCommerce.Tests;

public abstract class TestBase : IAsyncLifetime
{
    protected VerifySettings VerifySettings { get; } = new();
    protected ApplicationDbContext SeedContext { get; private set; } = null!;
    protected ApplicationDbContext VerifyContext { get; private set; } = null!;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    protected TestBase()
    {
        VerifySettings.IgnoreMember(nameof(StackTrace));
        VerifySettings.ScrubInlineGuids();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            //.LogTo(Console.WriteLine)
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
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
