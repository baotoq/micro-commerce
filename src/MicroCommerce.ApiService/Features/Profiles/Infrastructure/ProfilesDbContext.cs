using MicroCommerce.ApiService.Features.Profiles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Infrastructure;

public class ProfilesDbContext : DbContext
{
    public ProfilesDbContext(DbContextOptions<ProfilesDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema isolation - all Profiles tables in 'profiles' schema
        modelBuilder.HasDefaultSchema("profiles");

        // Apply configurations from Profiles module only
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProfilesDbContext).Assembly,
            t => t.Namespace?.Contains("Features.Profiles") == true);
    }
}
