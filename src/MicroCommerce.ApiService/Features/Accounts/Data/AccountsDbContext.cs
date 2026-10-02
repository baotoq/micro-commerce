using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MicroCommerce.ApiService.Features.Accounts.Data;

/// <summary>
/// The Accounts module's own context: its tables and migration history live in the
/// <see cref="Schema"/> schema, and no other module reads them (ADR-0003).
/// </summary>
public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    public const string Schema = "accounts";

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly, IsAccountsType);
    }

    /// <summary>Keeps this context's migration history apart from the other modules'.</summary>
    public static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schema);

    internal static bool IsAccountsType(Type type) =>
        type.Namespace?.StartsWith(typeof(Account).Namespace!, StringComparison.Ordinal) == true;
}
