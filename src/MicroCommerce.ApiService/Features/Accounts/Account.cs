using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>The identity a person signs in with, keyed by the Keycloak token subject.</summary>
public class Account
{
    public AccountId Id { get; init; } = AccountId.New();
    public required string Subject { get; init; }
    public string? Email { get; set; }
    public required string DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Subject).HasMaxLength(255).IsRequired();
        builder.HasIndex(a => a.Subject).IsUnique();
        builder.Property(a => a.Email).HasMaxLength(320);
        builder.Property(a => a.DisplayName).HasMaxLength(200).IsRequired();
    }
}

/// <summary>The signed-in Account and the Merchant it owns, if any, as returned by <c>GET /me</c>.</summary>
public record MeResponse(AccountId Id, string? Email, string DisplayName, bool IsPlatformOperator, MerchantResponse? Merchant)
{
    public static MeResponse From(Account a, bool isPlatformOperator, Merchant? merchant) =>
        new(a.Id, a.Email, a.DisplayName, isPlatformOperator, merchant is null ? null : MerchantResponse.From(merchant));
}
