using MicroCommerce.ApiService.Features.Catalog.Domain.Events;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.Entities;

/// <summary>
/// Category aggregate root for organizing products.
/// Reference implementation demonstrating:
/// - Factory method for creation
/// - Domain event raising
/// - Encapsulated state changes
/// </summary>
public sealed class Category : BaseAggregateRoot<CategoryId>
{
    public CategoryName Name { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // EF Core constructor
    private Category(CategoryId id) : base(id)
    {
        Name = default;
    }

    /// <summary>
    /// Factory method for creating a new category.
    /// Raises CategoryCreatedDomainEvent.
    /// </summary>
    public static Category Create(CategoryName name, string? description = null)
    {
        var category = new Category(CategoryId.New())
        {
            Name = name,
            Description = description?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        category.AddDomainEvent(new CategoryCreatedDomainEvent(category.Id));

        return category;
    }

    /// <summary>
    /// Updates the category details.
    /// </summary>
    public void Update(CategoryName name, string? description)
    {
        Name = name;
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
