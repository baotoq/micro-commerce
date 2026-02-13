using MicroCommerce.ApiService.Features.Reviews.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Infrastructure;

public class ReviewsDbContext : DbContext
{
    public ReviewsDbContext(DbContextOptions<ReviewsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Reviews tables in 'reviews' schema
        modelBuilder.HasDefaultSchema("reviews");

        // Apply configurations from Reviews module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReviewsDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Reviews") == true);
    }
}
