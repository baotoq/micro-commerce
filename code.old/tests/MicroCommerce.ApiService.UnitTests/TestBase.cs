using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace MicroCommerce.ApiService.UnitTests;

public abstract class TestBase : IAsyncLifetime
{
    protected ApplicationDbContext Context;
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .Build();
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        Context = new ApplicationDbContext(contextOptions);
        
        await Context.Database.EnsureCreatedAsync();
    }
    
    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }
}
