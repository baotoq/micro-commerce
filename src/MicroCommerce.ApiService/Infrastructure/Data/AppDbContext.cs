using MicroCommerce.ApiService.Infrastructure.Messaging;
using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SampleProcess> SampleProcesses => Set<SampleProcess>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // Typed IDs are stored as the primitive they wrap (research R2).
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
}
