using MicroCommerce.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Accounts.Data;

/// <summary>Index names the handlers map a unique violation (23505) back from (research R3).</summary>
public static class MerchantIndexes
{
    public const string AccountId = "ix_merchants_account_id";
    public const string NormalizedShopName = "ix_merchants_normalized_shop_name";
}

internal sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("merchants");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.AccountId).HasColumnName("account_id");
        builder.Property(m => m.ShopName).HasColumnName("shop_name").HasMaxLength(ShopName.MaxLength);
        builder.Property(m => m.NormalizedShopName).HasColumnName("normalized_shop_name").HasMaxLength(ShopName.MaxLength);
        builder.Property(m => m.Description).HasColumnName("description").HasMaxLength(Merchant.MaxDescriptionLength);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.Version).HasColumnName("version").IsConcurrencyToken();
        builder.Ignore(m => m.DomainEvents);

        builder.ComplexProperty(m => m.PickupAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("pickup_street").HasMaxLength(PickupAddress.MaxStreetLength);
            address.Property(a => a.District).HasColumnName("pickup_district").HasMaxLength(PickupAddress.MaxDistrictLength);
            address.Property(a => a.Province).HasColumnName("pickup_province").HasMaxLength(PickupAddress.MaxProvinceLength);
        });

        builder.HasIndex(m => m.AccountId).IsUnique().HasDatabaseName(MerchantIndexes.AccountId);
        builder.HasIndex(m => m.NormalizedShopName).IsUnique().HasDatabaseName(MerchantIndexes.NormalizedShopName);
    }
}
