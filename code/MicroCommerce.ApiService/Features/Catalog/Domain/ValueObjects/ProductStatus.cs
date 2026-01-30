namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

/// <summary>
/// Enum representing the lifecycle status of a product.
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Product is in draft state and not visible to customers.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Product is published and visible to customers.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Product has been archived (soft deleted).
    /// </summary>
    Archived = 2
}

