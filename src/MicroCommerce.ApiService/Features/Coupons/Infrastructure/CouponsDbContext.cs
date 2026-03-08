using MicroCommerce.ApiService.Common.Persistence;
using MicroCommerce.ApiService.Features.Coupons.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Infrastructure;

public class CouponsDbContext : BaseDbContext
{
    public CouponsDbContext(DbContextOptions<CouponsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Coupon tables in 'coupons' schema
        modelBuilder.HasDefaultSchema("coupons");

        // Apply configurations from Coupons module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CouponsDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Coupons") == true);
    }
}
