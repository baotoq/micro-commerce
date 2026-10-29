using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.ApiService.Features.Ordering;

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
                responses.Add(line.ToGoneResponse());
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

            responses.Add(variant.ToCartLineResponse(line.Quantity, lineTotal, flags));
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
