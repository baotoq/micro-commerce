using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MicroCommerce.ApiService.Features.Accounts.Data;

/// <summary>
/// Used by <c>dotnet ef</c> at design time, when Aspire hasn't injected a connection string.
/// </summary>
public class AccountsDbContextFactory : IDesignTimeDbContextFactory<AccountsDbContext>
{
    public AccountsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseNpgsql("Host=localhost;Database=micro-commerce-db", AccountsDbContext.ConfigureNpgsql)
            .Options;

        return new AccountsDbContext(options);
    }
}
