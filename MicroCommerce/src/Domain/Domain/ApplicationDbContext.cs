using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    
    public DbSet<Seller> Sellers { get; set; } = null!;
    public DbSet<Buyer> Buyers { get; set; } = null!;
}