using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.Catalog.Infrastructure;

public static class InfrastructureExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // Wire IEntityTypeConfiguration<T> implementations from this assembly into AppDbContext.
        // AppDbContext lives in Application and must not reference Infrastructure, so this static
        // bridge lets Infrastructure own model configuration without inverting the layer arrow.
        AppDbContext.ConfigureModel = modelBuilder =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);

        builder.AddNpgsqlDbContext<AppDbContext>("catalogdb",
            configureDbContextOptions: o => o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        return builder;
    }
}
