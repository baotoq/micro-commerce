using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

/// <summary>Identifies a Buyer's Checkout across modules (ADR-0005); Catalog keys Reservations by it (ADR-0006).</summary>
[ValueObject<Guid>]
public readonly partial struct CheckoutId
{
    public static CheckoutId New() => From(Guid.CreateVersion7());
}
