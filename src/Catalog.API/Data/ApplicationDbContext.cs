using Catalog.API.Data.Models;
using Data.UnitOfWork.EF;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Data
{
    public class ApplicationDbContext : BaseDbContext
    {
        public DbSet<Product> Products { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;

        public DbSet<Review> Reviews { get; set; } = null!;

        public DbSet<Reply> Replies { get; set; } = null!;

        public DbSet<Cart> Carts { get; set; } = null!;

        public DbSet<CartItem> CartItems { get; set; } = null!;

        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p!.Categories)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c!.Products)
                .HasForeignKey(pc => pc.CategoryId);
        }
    }
}
