using MicroCommerce.ApiService.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<CartItem>()
            .HasKey(bc => new { bc.CartId, bc.ProductId });
    }
    
    public DbSet<Buyer> Buyers { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<DeliveryAddress> DeliveryAddresses { get; set; } = null!;
    public DbSet<DeliveryOption> DeliveryOptions { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;
    public DbSet<Seller> Sellers { get; set; } = null!;
}