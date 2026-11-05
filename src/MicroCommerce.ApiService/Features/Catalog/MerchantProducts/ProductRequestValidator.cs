using FastEndpoints;
using FluentValidation;
using MicroCommerce.Domain.Catalog;

namespace MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

public sealed record OptionRequest(string Name, List<string> Values);

public sealed record VariantRequest(List<string> OptionValues, long? Price, int? Stock, int? WeightGrams);

/// <summary>What a Merchant sends to create or edit a Product (contracts/http-api.md).</summary>
public interface IProductRequest
{
    string Name { get; }

    string? Description { get; }

    Guid CategoryId { get; }

    List<OptionRequest> Options { get; }

    List<VariantRequest> Variants { get; }
}

public sealed record CreateProductRequest(
    string Name, string? Description, Guid CategoryId, List<OptionRequest> Options, List<VariantRequest> Variants) : IProductRequest;

public sealed record UpdateProductRequest(
    Guid Id, string Name, string? Description, Guid CategoryId, List<OptionRequest> Options, List<VariantRequest> Variants, int? Version)
    : IProductRequest;

public sealed class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator() => ProductRequestRules.Add(this);
}

public sealed class UpdateProductValidator : Validator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        ProductRequestRules.Add(this);
        RuleFor(x => x.Version).NotNull().GreaterThan(0).WithMessage("version is required.");
    }
}

/// <summary>The contract's Product validation rules, each reported on its own field (FR-018).</summary>
internal static class ProductRequestRules
{
    public static void Add<T>(AbstractValidator<T> v) where T : IProductRequest
    {
        v.RuleFor(x => x.Name).Must(n => n?.Trim().Length is > 0 and <= Product.MaxNameLength)
            .WithMessage($"name must be 1 to {Product.MaxNameLength} characters.");
        v.RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength)
            .WithMessage($"description must be at most {Product.MaxDescriptionLength} characters.");
        v.RuleFor(x => x.CategoryId).NotEmpty().WithMessage("categoryId is required.");

        // Null items are reported on their own index below; the set-wide checks skip them.
        v.RuleForEach(x => x.Options).NotNull().WithMessage("options must not contain null.");
        v.RuleFor(x => x.Options).NotNull().Custom((options, ctx) =>
        {
            if (options is null || options.Any(o => o is null)) return;
            foreach (var problem in OptionProblems(options)) ctx.AddFailure("Options", problem);
        });

        v.RuleFor(x => x.Variants).NotNull().Custom((variants, ctx) =>
        {
            var options = ctx.InstanceToValidate.Options;
            if (variants is null || options is null || options.Any(o => o is null) || OptionProblems(options).Any()) return;
            if (variants.Any(x => x?.OptionValues is null)) return;
            foreach (var problem in CombinationProblems(options, variants)) ctx.AddFailure("Variants", problem);
        });

        v.RuleForEach(x => x.Variants).NotNull().WithMessage("variants must not contain null.").ChildRules(variant =>
        {
            variant.RuleFor(x => x.OptionValues).NotNull()
                .WithMessage("optionValues is required: one value per option, or [] for the default Variant.");
            variant.RuleFor(x => x.Price).NotNull().InclusiveBetween(1, Money.MaxValue)
                .WithMessage($"price must be above 0 and at most {Money.MaxValue}.");
            variant.RuleFor(x => x.Stock).NotNull().GreaterThanOrEqualTo(0).WithMessage("stock must be 0 or more.");
            variant.RuleFor(x => x.WeightGrams).NotNull().GreaterThan(0).WithMessage("weightGrams must be above 0.");
        });
    }

    private static IEnumerable<string> OptionProblems(List<OptionRequest> options)
    {
        if (options.Count > ProductOption.MaxOptions)
            yield return $"A Product has at most {ProductOption.MaxOptions} options.";

        if (options.Select(o => o.Name?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count)
            yield return "Option names must be distinct.";

        foreach (var option in options)
        {
            if (!ValidText(option.Name, ProductOption.MaxNameLength))
                yield return $"Option names must be 1 to {ProductOption.MaxNameLength} characters, without control characters.";
            if (option.Values is null || option.Values.Count is 0 or > ProductOption.MaxValues)
            {
                yield return $"Option '{option.Name}' must have 1 to {ProductOption.MaxValues} values.";
                continue;
            }

            if (option.Values.Any(value => !ValidText(value, ProductOption.MaxValueLength)))
                yield return $"Values of option '{option.Name}' must be 1 to {ProductOption.MaxValueLength} characters, without control characters.";
            if (option.Values.Select(x => x?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != option.Values.Count)
                yield return $"Values of option '{option.Name}' must be distinct.";
        }
    }

    private static IEnumerable<string> CombinationProblems(List<OptionRequest> options, List<VariantRequest> variants)
    {
        var expected = Expand(options);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var variant in variants)
        {
            var values = variant.OptionValues ?? [];
            if (values.Count != options.Count || values.Where((value, i) => !options[i].Values.Contains(value?.Trim(), StringComparer.OrdinalIgnoreCase)).Any())
            {
                yield return $"Unknown combination: {Describe(values)}.";
                continue;
            }

            if (!seen.Add(Key(values)))
                yield return $"Duplicate combination: {Describe(values)}.";
        }

        foreach (var combination in expected.Where(c => !seen.Contains(Key(c))))
            yield return $"Missing combination: {Describe(combination)}.";
    }

    private static List<List<string>> Expand(List<OptionRequest> options) =>
        options.Aggregate(new List<List<string>> { new() }, (acc, option) =>
            acc.SelectMany(c => option.Values.Select(v => new List<string>(c) { v.Trim() })).ToList());

    private static string Key(IEnumerable<string> values) => string.Join('\u001f', values.Select(v => v.Trim().ToUpperInvariant()));

    private static string Describe(IReadOnlyCollection<string> values) => values.Count == 0 ? "(no option values)" : string.Join(" / ", values);

    private static bool ValidText(string? value, int maxLength) =>
        value?.Trim().Length is > 0 && value.Trim().Length <= maxLength && !value.Any(char.IsControl);

    /// <summary>The request's Options and Variants as domain inputs. Call only after validation.</summary>
    public static (List<ProductOption> Options, List<VariantSpec> Variants) ToDomain(this IProductRequest request) => (
        request.Options.Select(o => new ProductOption(o.Name, o.Values)).ToList(),
        request.Variants.Select(v => new VariantSpec(v.OptionValues, Money.From(v.Price!.Value), v.Stock!.Value, v.WeightGrams!.Value)).ToList());
}
