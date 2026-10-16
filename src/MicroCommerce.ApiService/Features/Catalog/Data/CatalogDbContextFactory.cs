using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

/// <summary>
/// Used by <c>dotnet ef</c> at design time, when Aspire hasn't injected a connection string.
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=micro-commerce-db", CatalogDbContext.ConfigureNpgsql)
            .Options;

        return new CatalogDbContext(options);
    }
}
