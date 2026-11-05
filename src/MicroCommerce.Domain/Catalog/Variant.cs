using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// One purchasable combination of a Product's Option values, with its own price, stock and weight.
/// Its identity survives edits that keep its combination (FR-013).
/// </summary>
public sealed class Variant
{
    private Variant()
    {
    }

    public VariantId Id { get; private init; }

    /// <summary>Its place in the Product's combination order: the last Option varies fastest.</summary>
    public int Position { get; internal set; }

    /// <summary>One value per Option, in Option order, spelt as the Option spells it. Empty for the default Variant.</summary>
    public List<string> OptionValues { get; private set; } = [];

    public Money Price { get; private set; }

    /// <summary>Units on hand, 0 or more. Buyers see Availability, never stock (FR-030).</summary>
    public int Stock { get; private set; }

    public int WeightGrams { get; private set; }

    internal static Variant Create(VariantId id, IReadOnlyList<string> optionValues, VariantSpec spec)
    {
        var variant = new Variant { Id = id };
        variant.Update(optionValues, spec);
        return variant;
    }

    internal void Update(IReadOnlyList<string> optionValues, VariantSpec spec)
    {
        if (spec.Stock < 0) throw new DomainRuleViolation("Stock must be 0 or more");
        if (spec.WeightGrams <= 0) throw new DomainRuleViolation("Weight must be above 0 grams");

        OptionValues = [.. optionValues];
        Price = spec.Price;
        Stock = spec.Stock;
        WeightGrams = spec.WeightGrams;
    }
}
