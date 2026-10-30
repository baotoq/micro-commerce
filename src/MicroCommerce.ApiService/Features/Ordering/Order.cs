using System.Text.Json.Serialization;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Ordering;

public class Order
{
    public required OrderId Id { get; init; }
    public required CheckoutId CheckoutId { get; init; }
    public required MerchantId MerchantId { get; init; }
    public OrderStatus Status { get; private set; } = OrderStatus.ToShip;
    public DateTimeOffset CreatedAt { get; init; } = DatabaseClock.UtcNow;
    public Checkout Checkout { get; private set; } = null!;

    public static Order FromGroup(CheckoutMerchant group) => new()
    {
        Id = OrderId.New(),
        CheckoutId = group.CheckoutId,
        MerchantId = group.MerchantId,
    };
}

[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]
public enum OrderStatus
{
    ToShip,
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(order => new { order.CheckoutId, order.MerchantId }).IsUnique();
        builder.HasOne(order => order.Checkout).WithMany().HasForeignKey(order => order.CheckoutId);
    }
}

public record OrderResponse(
    OrderId Id,
    CheckoutId CheckoutId,
    MerchantId MerchantId,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
    Money Subtotal,
    Money ShippingFee,
    DeliveryAddress DeliveryAddress,
    IReadOnlyList<OrderLineResponse> Lines);

public record OrderListResponse(IReadOnlyList<OrderResponse> Items, int Page, int PageSize, int TotalCount);
