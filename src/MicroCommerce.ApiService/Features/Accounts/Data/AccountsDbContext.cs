using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Wolverine.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Accounts.Data;

/// <summary>
/// The Accounts module's own context: its tables and migration history live in the
/// <see cref="Schema"/> schema, and no other module reads them (ADR-0003). It also maps Wolverine's envelope tables
/// (excluded from its migrations; Wolverine creates them), so the events it publishes through
/// <c>IDbContextOutbox&lt;AccountsDbContext&gt;</c> are saved by the same <c>SaveChangesAsync</c> as the change.
/// </summary>
public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    public const string Schema = "accounts";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Merchant> Merchants => Set<Merchant>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AccountId>().HaveConversion<AccountId.EfCoreValueConverter, AccountId.EfCoreValueComparer>();
        configurationBuilder.Properties<MerchantId>().HaveConversion<MerchantId.EfCoreValueConverter, MerchantId.EfCoreValueComparer>();
        configurationBuilder.Properties<ShopName>().HaveConversion<ShopName.EfCoreValueConverter, ShopName.EfCoreValueComparer>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly, IsAccountsType);
        modelBuilder.MapWolverineEnvelopeStorage(Messaging.Schema);
    }

    /// <summary>Keeps this context's migration history apart from the other modules'.</summary>
    public static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schema);

    private static bool IsAccountsType(Type type) =>
        type.Namespace?.StartsWith(typeof(Account).Namespace!, StringComparison.Ordinal) == true;
}
