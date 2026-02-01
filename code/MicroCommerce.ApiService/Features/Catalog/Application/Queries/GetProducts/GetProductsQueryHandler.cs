using MediatR;
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
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        // Apply filters
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == new CategoryId(request.CategoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ProductStatus>(request.Status, true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.Value.ToLower().Contains(searchLower) ||
                p.Description.ToLower().Contains(searchLower) ||
                (p.Sku != null && p.Sku.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var isDescending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "price" => isDescending
                ? query.OrderByDescending(p => p.Price.Amount)
                : query.OrderBy(p => p.Price.Amount),
            "name" => isDescending
                ? query.OrderByDescending(p => p.Name.Value)
                : query.OrderBy(p => p.Name.Value),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id.Value,
                p.Name.Value,
                p.Description,
                p.Price.Amount,
                p.Price.Currency,
                p.ImageUrl,
                p.Sku,
                p.Status.ToString(),
                p.CategoryId.Value,
                p.Category!.Name.Value,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new ProductListDto(items, totalCount, request.Page, request.PageSize);
    }
}

