using MicroCommerce.ApiService.Features.Catalog;
using Riok.Mapperly.Abstractions;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>Maps Ordering module data to its responses; every response member must have a source.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class OrderingMapper
{
    /// <summary>A line whose Variant the Catalog still has; <see cref="CartView"/> works out its total and flags.</summary>
    [MapperIgnoreSource(nameof(VariantLookup.WeightGrams))]
    [MapperIgnoreSource(nameof(VariantLookup.IsBuyable))]
    [MapProperty(nameof(VariantLookup.MerchantId), nameof(CartLineResponse.ShopId))]
    public static partial CartLineResponse ToCartLineResponse(
        this VariantLookup variant, int quantity, Money? lineTotal, IReadOnlyList<CartLineFlag> flags);

    /// <summary>A line whose Variant no longer exists: only what the Cart itself holds, flagged Unavailable.</summary>
    public static CartLineResponse ToGoneResponse(this CartLine line) =>
        new(line.VariantId, line.Quantity, null, null, [], null, null, null, null, null, [CartLineFlag.Unavailable]);
}
