using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts.Data;

/// <summary>
/// The Accounts area's data, in its own <c>accounts</c> schema with its own migration history. No other
/// area reads these tables (Principle II, research R2).
/// </summary>
public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    public const string Schema = "accounts";

    public DbSet<Merchant> Merchants => Set<Merchant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AccountsDbContext).Assembly,
            t => t.Namespace?.StartsWith(typeof(AccountsDbContext).Namespace!, StringComparison.Ordinal) == true);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInVogenEfCoreConverters();

    /// <summary>Keeps the migration history in this area's schema.</summary>
    public static void ConfigureNpgsql(Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Schema);
}
