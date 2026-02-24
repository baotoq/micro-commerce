using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

public sealed class PublishedProductsSpec : Specification<Product>
{
    public PublishedProductsSpec()
    {
        Query.Where(p => p.Status == ProductStatus.Published);
    }
}
