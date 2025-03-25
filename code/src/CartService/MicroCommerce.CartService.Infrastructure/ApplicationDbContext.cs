using System;
using MicroCommerce.CartService.Domain.Cart;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.CartService.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Cart> Carts { get; set; } = null!;
}
