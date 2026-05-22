using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductCountsQuery : IRequest<ProductCountsDto>;

public class GetProductCountsHandler(AppDbContext db) : IRequestHandler<GetProductCountsQuery, ProductCountsDto>
{
    public async Task<ProductCountsDto> Handle(GetProductCountsQuery request, CancellationToken ct)
    {
        // Single round trip. Replaces GroupBy(_ => 1).FirstOrDefaultAsync (which returns null
        // for an empty table) with conditional-sum aggregates that naturally return 0.
        var products = db.Products.AsNoTracking();

        var counts = await products
            .GroupBy(_ => 1)
            .Select(g => new ProductCountsDto(
                g.Count(),
                g.Count(p => p.Status == ProductStatus.Active),
                g.Count(p => p.Status == ProductStatus.Low),
                g.Count(p => p.Status == ProductStatus.Out),
                g.Count(p => p.Status == ProductStatus.Draft)))
            .FirstOrDefaultAsync(ct);

        return counts ?? new ProductCountsDto(0, 0, 0, 0, 0);
    }
}
