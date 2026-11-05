using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// A named way a Product varies (e.g. Colour), with its Option values (e.g. Red, Blue). A Product has
/// at most two. Names, and values within an Option, are distinct ignoring letter case (FR-010).
/// </summary>
public sealed record ProductOption
{
    public const int MaxOptions = 2;
    public const int MaxNameLength = 50;
    public const int MaxValues = 20;
    public const int MaxValueLength = 50;

    public ProductOption(string name, IReadOnlyList<string> values)
    {
        Name = Text(name, "An Option name");
        if (values is null || values.Count is 0 or > MaxValues)
            throw new DomainRuleViolation($"An Option has 1 to {MaxValues} values");

        var trimmed = values.Select(v => Text(v, "An Option value")).ToList();
        if (trimmed.Distinct(StringComparer.OrdinalIgnoreCase).Count() != trimmed.Count)
            throw new DomainRuleViolation($"The values of Option '{Name}' must be distinct");
        Values = trimmed;
    }

    public string Name { get; }

    public IReadOnlyList<string> Values { get; }

    public bool Equals(ProductOption? other) =>
        other is not null && Name == other.Name && Values.SequenceEqual(other.Values);

    public override int GetHashCode() => HashCode.Combine(Name, Values.Count);

    /// <summary>Checks a Product's Options together: at most two, with distinct names.</summary>
    public static void CheckSet(IReadOnlyList<ProductOption> options)
    {
        if (options.Count > MaxOptions)
            throw new DomainRuleViolation($"A Product has at most {MaxOptions} Options");
        if (options.Select(o => o.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count)
            throw new DomainRuleViolation("A Product's Option names must be distinct");
    }

    private static string Text(string value, string what)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > MaxValueLength || trimmed.Any(char.IsControl))
            throw new DomainRuleViolation($"{what} must be 1 to {MaxValueLength} characters, without control characters");
        return trimmed;
    }
}
