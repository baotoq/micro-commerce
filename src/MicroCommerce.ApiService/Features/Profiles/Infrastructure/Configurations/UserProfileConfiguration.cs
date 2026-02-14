using MicroCommerce.ApiService.Features.Profiles.Domain.Entities;
using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Profiles.Infrastructure.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.Id);

        // Strongly-typed ID conversion
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new UserProfileId(value))
            .ValueGeneratedNever();

        // UserId has unique index - prevents duplicate profiles per Keycloak user
        builder.HasIndex(p => p.UserId)
            .IsUnique();

        // DisplayName complex type (readonly record struct)
        builder.ComplexProperty(p => p.DisplayName, displayName =>
        {
            displayName.Property(d => d.Value)
                .HasColumnName("DisplayName")
                .IsRequired()
                .HasMaxLength(50);
        });

        // Optional avatar URL
        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(500);

        // PostgreSQL xmin for optimistic concurrency
        builder.Property(p => p.Version)
            .IsRowVersion();

        // Addresses owned collection
        builder.OwnsMany(p => p.Addresses, address =>
        {
            address.ToTable("Addresses", "profiles");

            address.WithOwner().HasForeignKey("UserProfileId");

            address.HasKey(nameof(Address.Id));

            // AddressId conversion
            address.Property(a => a.Id)
                .HasConversion(
                    id => id.Value,
                    value => new AddressId(value))
                .ValueGeneratedNever();

            address.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(50);

            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .IsRequired()
                .HasMaxLength(50);

            address.Property(a => a.ZipCode)
                .IsRequired()
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);
        });

        // Use backing field for addresses navigation
        builder.Navigation(p => p.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events (handled by interceptor)
        builder.Ignore(p => p.DomainEvents);
    }
}
