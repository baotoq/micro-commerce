using System.Text.Json.Serialization;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>
/// A Cart read live from the Catalog.
/// </summary>
/// <param name="ItemsTotal">
/// The sum of every line's <see cref="CartLineResponse.LineTotal"/>: lines that are Unavailable or
/// TotalTooLarge don't count; lines short of stock or from the Account's own Merchant still do.
/// </param>
public record CartResponse(IReadOnlyList<CartLineResponse> Lines, Money ItemsTotal);

/// <summary>
/// One Cart line with the Variant's live details. The Product, Shop and price are null when the Variant no longer
/// exists; a line that can't be checked out as it is has <see cref="Flags"/>.
/// </summary>
/// <param name="LineTotal">Unit price × quantity; null for an Unavailable or TotalTooLarge line.</param>
public record CartLineResponse(
    VariantId VariantId,
    int Quantity,
    ProductId? ProductId,
    string? ProductName,
    IReadOnlyList<string> OptionValues,
    MerchantId? ShopId,
    string? ShopName,
    Money? UnitPrice,
    Money? LineTotal,
    int? Available,
    IReadOnlyList<CartLineFlag> Flags);

/// <summary>Why a Cart line can't be checked out as it is.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CartLineFlag>))]
public enum CartLineFlag
{
    /// <summary>The Variant is gone, its Product is not Published, or its Merchant is suspended.</summary>
    Unavailable,

    /// <summary>The quantity is more than is available.</summary>
    InsufficientStock,

    /// <summary>The Variant belongs to the Account's own Merchant, which it can't buy from.</summary>
    OwnMerchant,

    /// <summary>Counting this line would take the Cart's total beyond the largest amount that can be paid.</summary>
    TotalTooLarge,
}

public static class CartView
{
    /// <summary>
    /// The Cart's <paramref name="lines"/> with their Variants' live details and flags, and its items total.
    /// </summary>
    /// <param name="ownMerchantId">The Account's own Merchant, if it has one.</param>
    public static CartResponse Build(
        IReadOnlyList<CartLine> lines, IReadOnlyDictionary<VariantId, VariantLookup> variants, MerchantId? ownMerchantId)
    {
        var total = 0L;
        var responses = new List<CartLineResponse>(lines.Count);
        foreach (var line in lines)
        {
            if (!variants.TryGetValue(line.VariantId, out var variant))
            {
                responses.Add(new(line.VariantId, line.Quantity, null, null, [], null, null, null, null, null, [CartLineFlag.Unavailable]));
                continue;
            }

            var flags = new List<CartLineFlag>();
            if (!variant.IsBuyable) flags.Add(CartLineFlag.Unavailable);
            if (line.Quantity > variant.Available) flags.Add(CartLineFlag.InsufficientStock);
            if (variant.MerchantId == ownMerchantId) flags.Add(CartLineFlag.OwnMerchant);

            Money? lineTotal = null;
            if (variant.IsBuyable)
            {
                if (TryAdd(total, variant.UnitPrice.Value, line.Quantity) is { } newTotal)
                {
                    lineTotal = Money.From(newTotal - total);
                    total = newTotal;
                }
                else
                {
                    flags.Add(CartLineFlag.TotalTooLarge);
                }
            }

            responses.Add(new(
                line.VariantId, line.Quantity, variant.ProductId, variant.ProductName, variant.OptionValues,
                variant.MerchantId, variant.ShopName, variant.UnitPrice, lineTotal, variant.Available, flags));
        }

        return new(responses, Money.From(total));
    }

    /// <returns><paramref name="total"/> plus <paramref name="price"/> × <paramref name="quantity"/>, or null if that
    /// is beyond <see cref="Product.MaxPrice"/>.</returns>
    private static long? TryAdd(long total, long price, int quantity)
    {
        try
        {
            var sum = checked(total + checked(price * quantity));
            return sum <= Product.MaxPrice ? sum : null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }
}
