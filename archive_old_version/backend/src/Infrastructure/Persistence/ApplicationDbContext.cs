using Domain;
using Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    
    public DbSet<Seller> Sellers { get; set; } = null!;
    public DbSet<Buyer> Buyers { get; set; } = null!;
    public DbSet<Shop> Shops { get; set; } = null!;
}