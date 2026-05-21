using MediatR;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductCountsQuery : IRequest<ProductCountsDto>;

public class GetProductCountsHandler(AppDbContext db) : IRequestHandler<GetProductCountsQuery, ProductCountsDto>
{
    public async Task<ProductCountsDto> Handle(GetProductCountsQuery request, CancellationToken ct)
    {
        var counts = await db.Products
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(p => p.Status == ProductStatus.Active),
                Low = g.Count(p => p.Status == ProductStatus.Low),
                Out = g.Count(p => p.Status == ProductStatus.Out),
                Draft = g.Count(p => p.Status == ProductStatus.Draft),
            })
            .FirstOrDefaultAsync(ct);

        return counts is null
            ? new ProductCountsDto(0, 0, 0, 0, 0)
            : new ProductCountsDto(counts.Total, counts.Active, counts.Low, counts.Out, counts.Draft);
    }
}
