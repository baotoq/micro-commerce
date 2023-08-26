using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<User> Users { get; set; }
}