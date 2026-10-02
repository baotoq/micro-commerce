using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Accounts entities belong to AccountsDbContext and its own schema.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly, t => !AccountsDbContext.IsAccountsType(t));
    }
}
