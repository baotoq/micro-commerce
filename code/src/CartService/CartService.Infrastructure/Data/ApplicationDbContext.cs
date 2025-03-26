using System.Reflection;
using MicroCommerce.CartService.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.CartService.Infrastructure.Data;

// dotnet ef migrations add Initial -s CartService.Api -p CartService.Infrastructure -o Data/Migrations --context ApplicationDbContext
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<Cart> Carts { get; set; } = null!;
}
