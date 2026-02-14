using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly CatalogDbContext _context;

    public GetProductByIdQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == new ProductId(request.Id))
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
                    p.Status.ToString(),
                    p.CategoryId.Value,
                    c.Name.Value,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.AverageRating,
                    p.ReviewCount))
            .FirstOrDefaultAsync(cancellationToken);

        return product;
    }
}

