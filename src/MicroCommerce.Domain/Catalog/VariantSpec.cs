namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// The Merchant's input for one Variant: its Option values, in the Product's Option order, and its
/// price, stock and weight. Creating or editing a Product turns specs into Variants.
/// </summary>
public sealed record VariantSpec(IReadOnlyList<string> OptionValues, Money Price, int Stock, int WeightGrams);
