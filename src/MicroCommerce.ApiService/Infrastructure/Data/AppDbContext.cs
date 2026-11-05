using MicroCommerce.ApiService.Infrastructure.Messaging;
using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SampleProcess> SampleProcesses => Set<SampleProcess>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    // Only the skeleton's own configurations: the Accounts and Catalog areas have their own contexts
    // and schemas (002 research R2).
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly, t => t.Namespace is { } ns
            && (ns.StartsWith("MicroCommerce.ApiService.Features.Diagnostics", StringComparison.Ordinal)
                || ns.StartsWith("MicroCommerce.ApiService.Infrastructure", StringComparison.Ordinal)));

    // Typed IDs are stored as the primitive they wrap (research R2).
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
}
