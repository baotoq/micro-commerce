using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Application.Specifications;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, ProductListDto>
{
    private readonly CatalogDbContext _context;

    public GetProductsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<ProductListDto> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve optional filter values
        CategoryId? categoryId = request.CategoryId.HasValue
            ? CategoryId.From(request.CategoryId.Value)
            : null;

        ProductStatus? productStatus = !string.IsNullOrWhiteSpace(request.Status) &&
            ProductStatus.TryFromName(request.Status, ignoreCase: true, out ProductStatus? parsed)
            ? parsed
            : null;

        // Build composed filter spec (category AND status AND search)
        GetProductsFilterSpec spec = new(categoryId, productStatus, request.Search);

        int totalCount = await _context.Products
            .AsNoTracking()
            .WithSpecification(spec)
            .CountAsync(cancellationToken);

        // Apply spec for filtering, then sort in handler (request-specific ordering)
        IQueryable<Product> sortedQuery = _context.Products
            .AsNoTracking()
            .WithSpecification(spec);

        bool isDescending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        sortedQuery = request.SortBy?.ToLowerInvariant() switch
        {
            "price" => isDescending
                ? sortedQuery.OrderByDescending(p => p.Price.Amount)
                : sortedQuery.OrderBy(p => p.Price.Amount),
            "name" => isDescending
                ? sortedQuery.OrderByDescending(p => p.Name.Value)
                : sortedQuery.OrderBy(p => p.Name.Value),
            "newest" => sortedQuery.OrderByDescending(p => p.CreatedAt),
            _ => sortedQuery.OrderByDescending(p => p.CreatedAt)
        };

        List<ProductDto> items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                _context.Categories.AsNoTracking(),
                p => p.CategoryId,
                c => c.Id,
                (p, c) => new ProductDto(
                    p.Id.Value,
                    p.Name.Value,
                    p.Description,
                    p.Price.Amount,
                    p.Price.Currency,
                    p.ImageUrl,
                    p.Sku,
                    p.Status.Name,
                    p.CategoryId.Value,
                    c.Name.Value,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.AverageRating,
                    p.ReviewCount))
            .ToListAsync(cancellationToken);

        return new ProductListDto(items, totalCount, request.Page, request.PageSize);
    }
}
