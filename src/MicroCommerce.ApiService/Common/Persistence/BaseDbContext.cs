using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Common.Persistence;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.RegisterAllInVogenEfCoreConverters();
        configurationBuilder.Conventions.Add(_ => new Conventions.AuditableConvention());
        configurationBuilder.Conventions.Add(_ => new Conventions.ConcurrencyTokenConvention());
        configurationBuilder.Conventions.Add(_ => new Conventions.SoftDeletableConvention());
    }
}
