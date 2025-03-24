using System;
using Microsoft.EntityFrameworkCore;
using MicroCommerce.CartService.Domain.Entities;

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
