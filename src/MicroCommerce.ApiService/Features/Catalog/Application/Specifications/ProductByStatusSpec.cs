using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

public sealed class ProductByStatusSpec : Specification<Product>
{
    public ProductByStatusSpec(ProductStatus status)
    {
        Query.Where(p => p.Status == status);
    }
}
