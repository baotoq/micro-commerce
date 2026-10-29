using System.Linq.Expressions;
using System.Text.Json.Serialization;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// Stock of a Variant held for a placed Checkout (ADR-0006). It refers to its Variant by ID with no foreign key, so a
/// Merchant can still edit a reserved Variant away; the Reservation then stays valid for Commit and Release.
/// </summary>
public class Reservation
{
    /// <summary>The states <see cref="Commit"/> moves from; a Held one is already committed.</summary>
    private static readonly ReservationState[] CommittableStates = [ReservationState.Reserved];

    /// <summary>The states <see cref="Release"/> moves from.</summary>
    private static readonly ReservationState[] ReleasableStates = [ReservationState.Reserved, ReservationState.Held];

    public required CheckoutId CheckoutId { get; init; }
    public required VariantId VariantId { get; init; }

    /// <summary>The Variant's Product, kept so its storefront cache entry can be evicted even once the Variant is gone.</summary>
    public required ProductId ProductId { get; init; }

    public required int Quantity { get; init; }

    public ReservationState State { get; private set; } = ReservationState.Reserved;

    /// <summary>When a Reserved row stops counting against availability; null once Held.</summary>
    public DateTimeOffset? HoldUntil { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DatabaseClock.UtcNow;

    public static Reservation Create(
        CheckoutId checkoutId, VariantId variantId, ProductId productId, int quantity, DateTimeOffset holdUntil) =>
        new()
        {
            CheckoutId = checkoutId,
            VariantId = variantId,
            ProductId = productId,
            Quantity = quantity,
            HoldUntil = holdUntil,
        };

    /// <summary>
    /// The Reservations that count against availability at <paramref name="now"/>: Held ones, and Reserved ones not
    /// past their <see cref="HoldUntil"/>. A lapsed one stops counting by itself, with no sweeper.
    /// </summary>
    public static Expression<Func<Reservation, bool>> ActiveAt(DateTimeOffset now) =>
        r => r.State == ReservationState.Held || (r.State == ReservationState.Reserved && r.HoldUntil > now);

    /// <summary>
    /// Makes a Reserved row permanent: Held, with no deadline. It takes one past its deadline too, since the deadline
    /// already includes a grace period after the payment window.
    /// </summary>
    /// <returns>Whether it is Held now: false if it was Released.</returns>
    public bool Commit()
    {
        if (CommittableStates.Contains(State))
        {
            State = ReservationState.Held;
            HoldUntil = null;
        }
        return State == ReservationState.Held;
    }

    /// <summary>Frees the stock; a Released one stays Released.</summary>
    public void Release()
    {
        if (ReleasableStates.Contains(State)) State = ReservationState.Released;
    }
}

/// <summary>
/// <c>Reserved</c> (until <see cref="Reservation.HoldUntil"/>) → <c>Held</c> → <c>Consumed</c> when its Order ships;
/// <c>Reserved</c> or <c>Held</c> → <c>Released</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ReservationState>))]
public enum ReservationState
{
    Reserved,
    Held,
    Consumed,
    Released,
}

public static class Availability
{
    /// <summary>
    /// What can be bought of a Variant: its on-hand stock minus its active Reservations, never below zero, since a
    /// Merchant can lower stock below what is already reserved.
    /// </summary>
    public static int Of(int stock, int reserved) => Math.Max(0, stock - reserved);
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => new { r.CheckoutId, r.VariantId });
        builder.Property(r => r.CheckoutId).ValueGeneratedNever();
        builder.Property(r => r.VariantId).ValueGeneratedNever();
        // A concurrent Commit and Release can't both apply to what each read.
        builder.Property(r => r.State).HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();
        builder.HasIndex(r => r.VariantId);
        builder.HasIndex(r => r.ProductId);
    }
}
