using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

public sealed class ProductsByCategorySpec : Specification<Product>
{
    public ProductsByCategorySpec(CategoryId categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId);
    }
}
