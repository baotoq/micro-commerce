using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// The Catalog's own copy of a Merchant's Shop profile, kept from the Accounts module's Merchant events by
/// <see cref="ShopHandler"/> (ADR-0003): the storefront reads Shops from here, never from Accounts' tables. It
/// arrives shortly after the Merchant opens, so a Merchant's Products can exist before its Shop does; the storefront
/// shows a Product only once its Shop is here.
/// </summary>
public class Shop
{
    /// <summary>The Merchant's ID: a Shop is a view of its Merchant, not a separate thing.</summary>
    public required MerchantId Id { get; init; }

    public required string ShopName { get; set; }
    public string? Description { get; set; }

    /// <summary>The Merchant's profile version this copy reflects; older events are ignored.</summary>
    public int ProfileVersion { get; set; }
}

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.ShopName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);
    }
}

/// <summary>A Merchant's Shop page header, as Buyers see it.</summary>
public record ShopResponse(MerchantId Id, string ShopName, string? Description);
