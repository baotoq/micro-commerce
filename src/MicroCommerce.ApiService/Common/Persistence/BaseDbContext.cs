using MicroCommerce.ApiService.Common.Persistence.Conventions;
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

        configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
        configurationBuilder.Conventions.Add(_ => new AuditableConvention());
        configurationBuilder.Conventions.Add(_ => new Conventions.ConcurrencyTokenConvention());
        configurationBuilder.Conventions.Add(_ => new SoftDeletableConvention());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}
