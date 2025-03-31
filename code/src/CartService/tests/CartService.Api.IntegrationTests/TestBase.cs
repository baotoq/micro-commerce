using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MicroCommerce.CartService.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace MicroCommerce.CartService.Api.IntegrationTests;

public class TestBase : IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext SeedContext;
    private readonly PostgreSqlContainer _postgresContainer;

    public TestBase()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("microcommerce_testdb")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add ApplicationDbContext using PostgreSQL container
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString());
                    });
                });
            });

        Client = Factory.CreateClient();
        SeedContext = Factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await SeedContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await SeedContext.Database.EnsureDeletedAsync();
        await SeedContext.DisposeAsync();
        await Factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}
