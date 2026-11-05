using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// The rule that a Product's Variants are exactly the combinations of its Option values (FR-011), and
/// the key that keeps a Variant's identity across edits (FR-013, research R5).
/// </summary>
public static class VariantCombinations
{
    /// <summary>Every combination of the Options' values, the last Option varying fastest. One empty combination without Options.</summary>
    public static IReadOnlyList<IReadOnlyList<string>> Expand(IReadOnlyList<ProductOption> options)
    {
        IEnumerable<IReadOnlyList<string>> combinations = [[]];
        foreach (var option in options)
        {
            combinations = combinations.SelectMany(c => option.Values.Select(v => (IReadOnlyList<string>)[.. c, v]));
        }

        return combinations.ToList();
    }

    /// <summary>
    /// A key for one combination that ignores Option order and letter case: the sorted upper-case
    /// <c>NAME=VALUE</c> pairs. Reordering Options keeps every key, so every Variant ID.
    /// </summary>
    public static string Key(IReadOnlyList<ProductOption> options, IReadOnlyList<string> values) =>
        string.Join('\u001f', options
            .Select((o, i) => $"{o.Name.ToUpperInvariant()}\u001e{values[i].ToUpperInvariant()}")
            .Order(StringComparer.Ordinal));

    /// <summary>
    /// Matches each spec to its combination, in combination order, spelt as the Options spell it.
    /// Throws when a combination is missing or duplicated, or a spec names an unknown value.
    /// </summary>
    public static IReadOnlyList<(IReadOnlyList<string> Values, VariantSpec Spec)> Match(
        IReadOnlyList<ProductOption> options, IReadOnlyList<VariantSpec> specs)
    {
        var combinations = Expand(options);
        var byKey = new Dictionary<string, VariantSpec>(StringComparer.Ordinal);
        foreach (var original in specs)
        {
            // Trimmed like Option values themselves, so " Red" matches Red.
            var spec = original with { OptionValues = original.OptionValues.Select(v => v?.Trim() ?? string.Empty).ToList() };
            if (spec.OptionValues.Count != options.Count)
                throw new DomainRuleViolation($"Each Variant needs one value per Option ({options.Count})");

            var unknown = spec.OptionValues.Where((v, i) => !options[i].Values.Contains(v, StringComparer.OrdinalIgnoreCase)).ToList();
            if (unknown.Count > 0)
                throw new DomainRuleViolation($"Unknown Option value: {string.Join(" / ", unknown)}");

            if (!byKey.TryAdd(Key(options, spec.OptionValues), spec))
                throw new DomainRuleViolation($"Duplicate Variant: {Describe(spec.OptionValues)}");
        }

        var missing = combinations.Where(c => !byKey.ContainsKey(Key(options, c))).ToList();
        if (missing.Count > 0)
            throw new DomainRuleViolation($"Missing Variant: {string.Join(", ", missing.Select(Describe))}");

        return combinations.Select(c => (c, byKey[Key(options, c)])).ToList();
    }

    public static string Describe(IReadOnlyList<string> values) => values.Count == 0 ? "(default)" : string.Join(" / ", values);
}
