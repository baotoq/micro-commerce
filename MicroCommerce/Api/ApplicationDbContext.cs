using Microsoft.EntityFrameworkCore;

namespace Api;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Cart> Carts { get; set; } = null!;
}

public class Cart
{
    public string Id { get; set; } = "";
}