using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Specifications;

public sealed class ProductsBaseSpec : Specification<Product>
{
    public ProductsBaseSpec()
    {
        // Identity spec — matches all products; serves as starting point for And() composition
    }
}
