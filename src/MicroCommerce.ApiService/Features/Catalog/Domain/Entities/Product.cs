using MicroCommerce.ApiService.Features.Catalog.Domain.Events;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.Entities;

/// <summary>
/// Product aggregate root for the catalog domain.
/// Demonstrates:
/// - Factory method for creation
/// - Domain event raising
/// - Encapsulated state changes
/// - Value objects for domain concepts
/// </summary>
public sealed class Product : BaseAggregateRoot<ProductId>
{
    public ProductName Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public string? Sku { get; private set; }
    public ProductStatus Status { get; private set; }
    public CategoryId CategoryId { get; private set; } = null!;
    public Category? Category { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Review statistics (denormalized)
    public decimal? AverageRating { get; private set; }
    public int ReviewCount { get; private set; }

    // EF Core constructor
    private Product(ProductId id) : base(id)
    {
    }

    /// <summary>
    /// Factory method for creating a new product.
    /// Products start in Draft status.
    /// Raises ProductCreatedDomainEvent.
    /// </summary>
    public static Product Create(
        ProductName name,
        string description,
        Money price,
        CategoryId categoryId,
        string? imageUrl = null,
        string? sku = null)
    {
        var product = new Product(ProductId.New())
        {
            Name = name,
            Description = description.Trim(),
            Price = price,
            CategoryId = categoryId,
            ImageUrl = imageUrl,
            Sku = sku?.Trim(),
            Status = ProductStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id));

        return product;
    }

    /// <summary>
    /// Updates the product details.
    /// Raises ProductUpdatedDomainEvent.
    /// </summary>
    public void Update(
        ProductName name,
        string description,
        Money price,
        CategoryId categoryId,
        string? imageUrl,
        string? sku)
    {
        Name = name;
        Description = description.Trim();
        Price = price;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        Sku = sku?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ProductUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Publishes the product, making it visible to customers.
    /// </summary>
    public void Publish()
    {
        if (Status == ProductStatus.Published) return;

        Status = ProductStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ProductStatusChangedDomainEvent(Id, Status));
    }

    /// <summary>
    /// Unpublishes the product, returning it to draft status.
    /// </summary>
    public void Unpublish()
    {
        if (Status == ProductStatus.Draft) return;

        Status = ProductStatus.Draft;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ProductStatusChangedDomainEvent(Id, Status));
    }

    /// <summary>
    /// Archives the product (soft delete).
    /// Archived products are not visible to customers.
    /// </summary>
    public void Archive()
    {
        if (Status == ProductStatus.Archived) return;

        Status = ProductStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ProductArchivedDomainEvent(Id));
    }

    /// <summary>
    /// Updates the denormalized review statistics.
    /// Called by review event handlers.
    /// </summary>
    public void UpdateReviewStats(decimal? averageRating, int reviewCount)
    {
        AverageRating = averageRating;
        ReviewCount = reviewCount;
    }
}

