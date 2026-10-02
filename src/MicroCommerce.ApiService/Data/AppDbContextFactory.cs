using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MicroCommerce.ApiService.Data;

/// <summary>
/// Used by <c>dotnet ef</c> at design time, when Aspire hasn't injected a connection string.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=micro-commerce-db")
            .Options;

        return new AppDbContext(options);
    }
}
