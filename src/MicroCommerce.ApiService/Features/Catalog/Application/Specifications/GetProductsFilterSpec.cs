using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

/// <summary>
/// Composite specification for GetProducts query filtering.
/// Applies category, status, and search filters as AND conditions.
/// Sorting and pagination remain in the handler (per spec pattern best practices).
/// </summary>
public sealed class GetProductsFilterSpec : Specification<Product>
{
    public GetProductsFilterSpec(
        CategoryId? categoryId = null,
        ProductStatus? status = null,
        string? searchTerm = null)
    {
        if (categoryId is not null)
            Query.Where(p => p.CategoryId == categoryId);

        if (status is not null)
            Query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string lower = searchTerm.ToLower();
            Query.Where(p =>
                p.Name.Value.ToLower().Contains(lower) ||
                p.Description.ToLower().Contains(lower) ||
                (p.Sku != null && p.Sku.ToLower().Contains(lower)));
        }
    }
}
