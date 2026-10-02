using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// A selling business on the Marketplace, owned by exactly one Account. Its Shop is the public view of it.
/// </summary>
public class Merchant
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required Guid AccountId { get; init; }
    private string _shopName = null!;

    public required string ShopName
    {
        get => _shopName;
        set
        {
            _shopName = value;
            NormalizedShopName = NormalizeShopName(value);
        }
    }

    /// <summary>The Shop name with case folded away; unique, so Shop names are unique ignoring case.</summary>
    public string NormalizedShopName { get; private set; } = null!;
    public string? Description { get; set; }
    public required PickupAddress PickupAddress { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public static string NormalizeShopName(string shopName) => shopName.ToUpperInvariant();
}

/// <summary>Where the Merchant's parcels ship from; the province is what the Carrier quotes from.</summary>
public record PickupAddress(string Street, string Ward, string District, string Province);

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public const string ShopNameIndex = "IX_Merchants_NormalizedShopName";
    public const string AccountIdIndex = "IX_Merchants_AccountId";

    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.HasKey(m => m.Id);

        // One Merchant per Account.
        builder.HasOne<Account>().WithOne().HasForeignKey<Merchant>(m => m.AccountId);
        builder.HasIndex(m => m.AccountId).IsUnique().HasDatabaseName(AccountIdIndex);

        builder.Property(m => m.ShopName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.NormalizedShopName).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => m.NormalizedShopName).IsUnique().HasDatabaseName(ShopNameIndex);

        builder.Property(m => m.Description).HasMaxLength(2000);

        builder.ComplexProperty(m => m.PickupAddress, a =>
        {
            a.Property(x => x.Street).HasMaxLength(200).IsRequired();
            a.Property(x => x.Ward).HasMaxLength(100).IsRequired();
            a.Property(x => x.District).HasMaxLength(100).IsRequired();
            a.Property(x => x.Province).HasMaxLength(100).IsRequired();
        });
    }
}

/// <summary>A Merchant's Shop profile, as its owner sees it.</summary>
public record MerchantResponse(
    Guid Id, string ShopName, string? Description, PickupAddress PickupAddress, DateTimeOffset CreatedAt)
{
    public static MerchantResponse From(Merchant m) =>
        new(m.Id, m.ShopName, m.Description, m.PickupAddress, m.CreatedAt);
}

public class PickupAddressValidator : AbstractValidator<PickupAddress>
{
    public PickupAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Ward).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(100);
    }
}

/// <summary>Maps a unique-index violation on Merchants to the rule it enforces.</summary>
internal static class MerchantUniqueness
{
    public const string ShopNameTaken = "This Shop name is already taken.";
    public const string AlreadyOwnsMerchant = "This Account already owns a Merchant.";

    public static string? ViolatedIndex(DbUpdateException e) =>
        e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg
            ? pg.ConstraintName
            : null;
}
