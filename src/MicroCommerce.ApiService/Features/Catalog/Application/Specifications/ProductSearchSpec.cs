using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

public sealed class ProductSearchSpec : Specification<Product>
{
    public ProductSearchSpec(string searchTerm)
    {
        string lower = searchTerm.ToLower();
        Query.Where(p =>
            p.Name.Value.ToLower().Contains(lower) ||
            p.Description.ToLower().Contains(lower) ||
            (p.Sku != null && p.Sku.ToLower().Contains(lower)));
    }
}
