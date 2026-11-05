using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// A node in the Marketplace-wide Category tree, maintained by Platform Operators. A Category's parent
/// is set when it is created and never changes. The rules that involve Products (only leaves hold
/// Products; a Category holding Products gains no children; only empty leaves are deleted) need
/// the Products, so the Catalog's handlers enforce them under a row lock (research R4).
/// </summary>
public sealed class Category : AggregateRoot<CategoryId>
{
    public const int MaxNameLength = 100;

    private Category()
    {
    }

    public string Name { get; private set; } = null!;

    /// <summary>The name in upper case, so sibling names are unique ignoring letter case.</summary>
    public string NormalizedName { get; private set; } = null!;

    public CategoryId? ParentId { get; private init; }

    public static Category CreateTopLevel(CategoryId id, string name) => Create(id, name, parentId: null);

    public static Category CreateChild(CategoryId id, string name, CategoryId parentId) => Create(id, name, parentId);

    public void Rename(string name) => SetName(name);

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    private static Category Create(CategoryId id, string name, CategoryId? parentId)
    {
        var category = new Category { Id = id, ParentId = parentId };
        category.SetName(name);
        return category;
    }

    private void SetName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > MaxNameLength)
            throw new DomainRuleViolation($"A Category name must be 1 to {MaxNameLength} characters");

        Name = trimmed;
        NormalizedName = trimmed.ToUpperInvariant();
    }
}
