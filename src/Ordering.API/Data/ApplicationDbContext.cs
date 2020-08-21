using Data.UnitOfWork.EF;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Data.Models;

namespace Ordering.API.Data
{
    public class ApplicationDbContext : BaseDbContext
    {
        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
