using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

/// <summary>
/// Provides an AppDbContext for `dotnet ef migrations add` invocations. The runtime
/// wiring lives in InfrastructureExtensions; design-time tooling can't reach that DI graph,
/// so we apply the same Infrastructure assembly's IEntityTypeConfiguration<T> set here.
/// </summary>
public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        AppDbContext.ConfigureModel = modelBuilder =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=catalogdb;Username=postgres;Password=designtime",
                npg => npg.MigrationsAssembly(typeof(ProductConfiguration).Assembly.FullName))
            .Options;
        return new AppDbContext(options);
    }
}
