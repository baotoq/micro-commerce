using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductsQuery(int Page, int PageSize, string? Status, string? Search) : IRequest<PagedResult<ProductDto>>;

public class GetProductsHandler(AppDbContext db) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();

        if (ParseStatus(request.Status) is { } status)
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search;
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{search}%") ||
                EF.Functions.ILike(p.Category, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.Views7d)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Sku.Value,
                p.Name,
                p.Category,
                p.Price,
                p.Inventory,
                p.Status == ProductStatus.Active ? "active"
                    : p.Status == ProductStatus.Low ? "low"
                    : p.Status == ProductStatus.Out ? "out"
                    : "draft",
                p.Views7d,
                p.Description,
                p.Tags,
                p.Weight,
                p.Origin,
                p.PhotoUrls))
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, total, request.Page, request.PageSize);
    }

    private static ProductStatus? ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "active" => ProductStatus.Active,
        "low" => ProductStatus.Low,
        "out" => ProductStatus.Out,
        "draft" => ProductStatus.Draft,
        _ => null,
    };
}
