using FluentValidation;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// The rule that a Product's Variants are exactly the combinations of its options' values, each once. Option names
/// and values compare ignoring case, so "red" names the same value as "Red".
/// </summary>
internal static class VariantCombinations
{
    public static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Every combination of one value of each option, in option order with the last option varying fastest; a single
    /// empty combination when there are no options.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<string>> Of(IReadOnlyList<ProductOption> options) =>
        options.Aggregate<ProductOption, IReadOnlyList<IReadOnlyList<string>>>(
            [[]],
            (combinations, option) => [.. combinations.SelectMany(c => option.Values.Select(v => (IReadOnlyList<string>)[.. c, v]))]);

    /// <summary>
    /// Identifies a combination by its option names and values rather than their order, so a Variant keeps its ID when
    /// the options are reordered.
    /// </summary>
    public static string Key(IReadOnlyList<ProductOption> options, IReadOnlyList<string> values) =>
        string.Join('\u001f', options
            .Zip(values, (option, value) => $"{option.Name.ToUpperInvariant()}\u001e{value.ToUpperInvariant()}")
            .Order(StringComparer.Ordinal));

    /// <summary>The <paramref name="values"/>, each spelled as its option spells it.</summary>
    public static IReadOnlyList<string> InOptionSpelling(IReadOnlyList<ProductOption> options, IReadOnlyList<string> values) =>
        [.. values.Select((value, i) => options[i].Values.FirstOrDefault(v => Comparer.Equals(v, value.Trim())) ?? value.Trim())];

    /// <summary>Whether no two of the texts are the same, ignoring case and surrounding spaces; blanks aren't compared.</summary>
    public static bool AllDifferent(IEnumerable<string?> texts)
    {
        var present = texts.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t!.Trim()).ToList();
        return present.Distinct(Comparer).Count() == present.Count;
    }

    private static string Label(IReadOnlyList<string> combination) => string.Join(" / ", combination);

    /// <summary>
    /// Checks the Variants against the options. It only runs once the options and Variants are well formed, since the
    /// other rules already report what's wrong with them.
    /// </summary>
    public static void Validate(IReadOnlyList<VariantRequest> variants, ValidationContext<IProductDetails> context)
    {
        if (variants is null || variants.Count == 0 || variants.Any(v => v is null)) return;
        if (WellFormed(context.InstanceToValidate.Options ?? []) is not { } options) return;

        if (options.Count == 0 && variants.Count > 1)
        {
            context.AddFailure(context.PropertyPath, "A Product without options has exactly one Variant.");
            return;
        }

        // Named through FluentValidation's resolver, as its own rules are, so FastEndpoints camelCases them too.
        var optionValuesName = ValidatorOptions.Global.PropertyNameResolver(
            typeof(VariantRequest), typeof(VariantRequest).GetProperty(nameof(VariantRequest.OptionValues)), null);
        var seen = new HashSet<string>();
        for (var i = 0; i < variants.Count; i++)
        {
            var property = $"{context.PropertyPath}[{i}].{optionValuesName}";
            var values = variants[i].OptionValues ?? [];
            if (values.Count != options.Count)
            {
                context.AddFailure(property, options.Count == 0
                    ? "A Product without options has a Variant without option values."
                    : $"Give one value for each option: {string.Join(", ", options.Select(o => o.Name))}.");
                continue;
            }

            var unknown = values
                .Select((value, j) => (value, option: options[j]))
                .FirstOrDefault(x => x.value is null || !x.option.Values.Contains(x.value.Trim(), Comparer));
            if (unknown.option is not null)
            {
                context.AddFailure(property, $"'{unknown.value}' is not a value of {unknown.option.Name}.");
                continue;
            }

            if (!seen.Add(Key(options, [.. values.Select(v => v.Trim())])))
                context.AddFailure(property, "Another Variant has the same option values.");
        }

        // Without options the one Variant's values are already reported above.
        var missing = Of(options).Where(c => !seen.Contains(Key(options, c))).ToList();
        if (options.Count > 0 && missing.Count > 0)
            context.AddFailure(context.PropertyPath, $"Missing a Variant for {string.Join(", ", missing.Select(Label))}.");
    }

    /// <returns>The options, trimmed, if they satisfy their own rules; otherwise null.</returns>
    private static List<ProductOption>? WellFormed(IReadOnlyList<ProductOption> options)
    {
        if (options.Count > Product.MaxOptions) return null;
        if (options.Any(o => string.IsNullOrWhiteSpace(o?.Name)
                || o.Values is not { Count: > 0 and <= ProductOption.MaxValues }
                || o.Values.Any(string.IsNullOrWhiteSpace)))
            return null;

        var distinct = AllDifferent(options.Select(o => o.Name)) && options.All(o => AllDifferent(o.Values));
        return distinct ? [.. options.Select(o => new ProductOption(o.Name.Trim(), [.. o.Values.Select(v => v.Trim())]))] : null;
    }
}
