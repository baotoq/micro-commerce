using System.Text.Json.Serialization;
using FluentValidation;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

namespace MicroCommerce.ApiService.Features.Ordering;

public class Checkout
{
    public required CheckoutId Id { get; init; }
    public required AccountId AccountId { get; init; }
    public CheckoutStatus Status { get; private set; } = CheckoutStatus.AwaitingPayment;
    public required DateTimeOffset PlacedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required DeliveryAddress DeliveryAddress { get; init; }
    public required Money ItemsTotal { get; init; }
    public required Money ShippingTotal { get; init; }
    public required Money Total { get; init; }
    public List<CheckoutMerchant> Merchants { get; } = [];
    public List<OrderLine> Lines { get; } = [];

    public bool Expire()
    {
        if (Status != CheckoutStatus.AwaitingPayment) return false;
        Status = CheckoutStatus.Expired;
        return true;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter<CheckoutStatus>))]
public enum CheckoutStatus
{
    AwaitingPayment,
    Expired,
}

public record DeliveryAddress
{
    public const int RecipientNameMaxLength = 100;
    public const int PhoneMaxLength = 30;
    public const int StreetMaxLength = 200;
    public const int WardMaxLength = 100;
    public const int ProvinceMaxLength = 100;

    public required string RecipientName { get; init; }
    public required string Phone { get; init; }
    public required string Street { get; init; }
    public required string Ward { get; init; }
    public required string Province { get; init; }
}

public static class VietnamProvinces
{
    // The Government's list of 34 units after the 2025 reorganization; the two-tier model has no district level.
    // https://en.baochinhphu.vn/names-and-administrative-centers-of-34-provinces-and-centrally-run-cities-specified-111250415094350491.htm
    public static readonly IReadOnlyList<string> All =
    [
        "An Giang", "Bắc Ninh", "Cà Mau", "Cao Bằng", "Cần Thơ", "Đà Nẵng", "Đắk Lắk", "Điện Biên",
        "Đồng Nai", "Đồng Tháp", "Gia Lai", "Hà Nội", "Hà Tĩnh", "Hải Phòng", "Hồ Chí Minh", "Huế",
        "Hưng Yên", "Khánh Hòa", "Lai Châu", "Lạng Sơn", "Lào Cai", "Lâm Đồng", "Nghệ An", "Ninh Bình",
        "Phú Thọ", "Quảng Ngãi", "Quảng Ninh", "Quảng Trị", "Sơn La", "Tây Ninh", "Thái Nguyên",
        "Thanh Hóa", "Tuyên Quang", "Vĩnh Long",
    ];
}

public class DeliveryAddressValidator : AbstractValidator<DeliveryAddress>
{
    public DeliveryAddressValidator()
    {
        RuleFor(a => a.RecipientName).NotEmpty().MaximumLength(DeliveryAddress.RecipientNameMaxLength);
        RuleFor(a => a.Phone)
            .NotEmpty()
            .MaximumLength(DeliveryAddress.PhoneMaxLength)
            .Matches(@"^\+?[0-9][0-9\s().-]{6,28}$");
        RuleFor(a => a.Street).NotEmpty().MaximumLength(DeliveryAddress.StreetMaxLength);
        RuleFor(a => a.Ward).NotEmpty().MaximumLength(DeliveryAddress.WardMaxLength);
        RuleFor(a => a.Province).Must(VietnamProvinces.All.Contains)
            .WithMessage("Choose a province from the supported list.");
    }
}

public class CheckoutMerchant
{
    public required CheckoutId CheckoutId { get; init; }
    public required MerchantId MerchantId { get; init; }
    public required Money Subtotal { get; init; }
    public required Money ShippingFee { get; init; }
}

public class OrderLine
{
    public required CheckoutId CheckoutId { get; init; }
    public required VariantId VariantId { get; init; }
    public required MerchantId MerchantId { get; init; }
    public required ProductId ProductId { get; init; }
    public required string ProductName { get; init; }
    public required string[] OptionValues { get; init; }
    public required Money UnitPrice { get; init; }
    public required int Quantity { get; init; }
}

public class CheckoutConfiguration : IEntityTypeConfiguration<Checkout>
{
    public void Configure(EntityTypeBuilder<Checkout> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(c => c.AccountId).IsUnique().HasFilter("\"Status\" = 'AwaitingPayment'");
        builder.OwnsOne(c => c.DeliveryAddress, a =>
        {
            a.Property(x => x.RecipientName).HasMaxLength(DeliveryAddress.RecipientNameMaxLength).IsRequired();
            a.Property(x => x.Phone).HasMaxLength(DeliveryAddress.PhoneMaxLength).IsRequired();
            a.Property(x => x.Street).HasMaxLength(DeliveryAddress.StreetMaxLength).IsRequired();
            a.Property(x => x.Ward).HasMaxLength(DeliveryAddress.WardMaxLength).IsRequired();
            a.Property(x => x.Province).HasMaxLength(DeliveryAddress.ProvinceMaxLength).IsRequired();
        });
        builder.HasMany(c => c.Merchants).WithOne().HasForeignKey(m => m.CheckoutId);
        builder.HasMany(c => c.Lines).WithOne().HasForeignKey(l => l.CheckoutId);
    }
}

public class CheckoutMerchantConfiguration : IEntityTypeConfiguration<CheckoutMerchant>
{
    public void Configure(EntityTypeBuilder<CheckoutMerchant> builder)
    {
        builder.HasKey(m => new { m.CheckoutId, m.MerchantId });
        builder.Property(m => m.MerchantId).ValueGeneratedNever();
    }
}

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(l => new { l.CheckoutId, l.VariantId });
        builder.Property(l => l.VariantId).ValueGeneratedNever();
        builder.Property(l => l.OptionValues).HasColumnType("text[]").IsRequired();
        builder.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
    }
}

public record CheckoutMerchantResponse(MerchantId MerchantId, Money Subtotal, Money ShippingFee);

public record OrderLineResponse(
    VariantId VariantId,
    MerchantId MerchantId,
    ProductId ProductId,
    string ProductName,
    IReadOnlyList<string> OptionValues,
    Money UnitPrice,
    int Quantity);

public record CheckoutResponse(
    CheckoutId Id,
    CheckoutStatus Status,
    DateTimeOffset PlacedAt,
    DateTimeOffset ExpiresAt,
    DeliveryAddress DeliveryAddress,
    Money ItemsTotal,
    Money ShippingTotal,
    Money Total,
    IReadOnlyList<CheckoutMerchantResponse> Merchants,
    IReadOnlyList<OrderLineResponse> Lines);

public record CheckoutListResponse(IReadOnlyList<CheckoutResponse> Items, int Page, int PageSize, int TotalCount);

public record CheckoutConflictResponse(string Detail, IReadOnlyList<CheckoutLineProblem> Lines, CheckoutId? ExistingCheckoutId = null);

public record CheckoutLineProblem(VariantId VariantId, string Reason, int? Available = null);

public sealed record OrderingOptions
{
    public TimeSpan PaymentWindow { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan ReservationGrace { get; init; } = TimeSpan.FromMinutes(5);
    public long ShippingFee { get; init; } = 30_000;
}

public interface IShippingFeeQuote
{
    Money Quote(MerchantId merchantId);
}

public sealed class FlatShippingFeeQuote(IOptions<OrderingOptions> options) : IShippingFeeQuote
{
    public Money Quote(MerchantId merchantId) => Money.From(options.Value.ShippingFee);
}
