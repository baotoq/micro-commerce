using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// The stock-holding half of the Catalog's in-process contract (ADR-0006; <see cref="ICatalogLookup"/> is the
/// other). IDs and plain values only, and every operation is idempotent: repeating one for a Checkout is a no-op.
/// </summary>
public interface ICatalogReservations
{
    /// <summary>
    /// Holds stock for every line of a Checkout until <paramref name="holdUntil"/>, all or nothing: if any line's
    /// Variant isn't buyable or hasn't enough available, nothing is reserved and every failing line is returned. A
    /// Checkout that already has Reservations is left as it is and succeeds, whatever their state: Ordering doesn't
    /// Reserve again for a Checkout it has released or let expire.
    /// </summary>
    Task<ReserveResult> ReserveAsync(
        CheckoutId checkoutId, IReadOnlyCollection<ReservationLine> lines, DateTimeOffset holdUntil, CancellationToken ct);

    /// <summary>
    /// Makes the Checkout's Reservations permanent (Held, no deadline), even past their deadline: the deadline Ordering
    /// passes already includes a grace period after the payment window, so a payment that arrives in it keeps its
    /// stock. If the stock was taken after the deadline anyway, availability shows zero rather than going negative.
    /// </summary>
    /// <returns>Whether the Checkout's Reservations are Held now: false if it has none or they were Released.</returns>
    Task<bool> CommitAsync(CheckoutId checkoutId, CancellationToken ct);

    /// <summary>Frees the Checkout's Reserved or Held stock.</summary>
    Task ReleaseAsync(CheckoutId checkoutId, CancellationToken ct);
}

/// <summary>A quantity of one Variant to reserve; a Variant listed twice is reserved once, for the sum.</summary>
public record ReservationLine(VariantId VariantId, int Quantity);

/// <summary>Why a line couldn't be reserved, and how many were available when it was tried.</summary>
public record ReservationFailure(VariantId VariantId, ReservationFailureReason Reason, int Available);

public enum ReservationFailureReason
{
    /// <summary>The Variant is gone, its Product isn't Published, or its Merchant is suspended.</summary>
    NotBuyable,

    /// <summary>Fewer are available than the line asks for.</summary>
    NotEnoughAvailable,
}

/// <summary>The lines that couldn't be reserved; none when the Reserve succeeded.</summary>
public record ReserveResult(IReadOnlyList<ReservationFailure> Failures)
{
    public bool Succeeded => Failures.Count == 0;
}

public class CatalogReservations(CatalogDbContext db, ICatalogLookup lookup, StorefrontProductCache storefrontCache)
    : ICatalogReservations
{
    public async Task<ReserveResult> ReserveAsync(
        CheckoutId checkoutId, IReadOnlyCollection<ReservationLine> lines, DateTimeOffset holdUntil, CancellationToken ct)
    {
        if (lines.Count == 0) throw new ArgumentException("A Reserve needs at least one line.", nameof(lines));
        if (lines.Any(l => l.Quantity <= 0)) throw new ArgumentException("Every line's quantity must be positive.", nameof(lines));

        var quantities = lines.GroupBy(l => l.VariantId).ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
        var variantIds = quantities.Keys.Select(id => id.Value).ToArray();
        holdUntil = holdUntil.ToUniversalTime();

        var reservedProductIds = new List<ProductId>();
        // Aspire's retrying execution strategy refuses an explicit transaction outside it, and runs the whole
        // unit again on a transient failure.
        var result = await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            reservedProductIds.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            // Raw SQL because EF Core has no locking. First this Checkout's own lock, so a concurrent repeat of it
            // waits and then finds the first, whatever lines it carries; it is never held while waiting for another
            // Checkout's, so it can't deadlock.
            var checkoutKey = checkoutId.Value.ToString();
            await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({checkoutKey}, 0))", ct);

            // Then the Variant rows, in ID order, so two Checkouts sharing Variants wait for each other instead of
            // deadlocking, and one racing for the last unit reads availability only once the other has committed.
            await db.Database.ExecuteSqlAsync(
                $"""SELECT 1 FROM catalog."Variants" WHERE "Id" = ANY({variantIds}) ORDER BY "Id" FOR UPDATE""", ct);

            if (await db.Reservations.AnyAsync(r => r.CheckoutId == checkoutId, ct))
                return new ReserveResult([]);

            var variants = await lookup.LookupAsync(quantities.Keys, ct);
            var failures = quantities
                .Select(line => variants.GetValueOrDefault(line.Key) switch
                {
                    null or { IsBuyable: false } => new ReservationFailure(line.Key, ReservationFailureReason.NotBuyable, 0),
                    { Available: var available } when available < line.Value =>
                        new ReservationFailure(line.Key, ReservationFailureReason.NotEnoughAvailable, available),
                    _ => null,
                })
                .OfType<ReservationFailure>()
                .ToList();
            if (failures.Count > 0) return new ReserveResult(failures);

            foreach (var (variantId, quantity) in quantities)
            {
                var productId = variants[variantId].ProductId;
                db.Reservations.Add(Reservation.Create(checkoutId, variantId, productId, quantity, holdUntil));
                reservedProductIds.Add(productId);
            }
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return new ReserveResult([]);
        });

        await storefrontCache.EvictAsync(reservedProductIds.Distinct(), ct);
        return result;
    }

    public async Task<bool> CommitAsync(CheckoutId checkoutId, CancellationToken ct)
    {
        var reservations = await ChangeAsync(checkoutId, r => r.Commit(), ct);
        return reservations.Count > 0 && reservations.All(r => r.State == ReservationState.Held);
    }

    public async Task ReleaseAsync(CheckoutId checkoutId, CancellationToken ct) =>
        await ChangeAsync(checkoutId, r => r.Release(), ct);

    /// <summary>
    /// Applies <paramref name="change"/> to each of the Checkout's Reservations and saves them. <see
    /// cref="Reservation.State"/> is their concurrency token, so if a concurrent Commit or Release got there first, it
    /// reads them again and reapplies the change to what that left.
    /// </summary>
    /// <returns>The Checkout's Reservations as saved.</returns>
    private async Task<List<Reservation>> ChangeAsync(CheckoutId checkoutId, Action<Reservation> change, CancellationToken ct)
    {
        const int attempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            db.ChangeTracker.Clear();
            var reservations = await db.Reservations.Where(r => r.CheckoutId == checkoutId).ToListAsync(ct);
            reservations.ForEach(change);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt < attempts)
            {
                continue;
            }

            // What Buyers see of the Products' available stock may have changed.
            await storefrontCache.EvictAsync(reservations.Select(r => r.ProductId).Distinct(), ct);
            return reservations;
        }
    }
}
