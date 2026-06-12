using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductCategoriesQuery() : IRequest<IReadOnlyList<string>>;

public class GetProductCategoriesHandler(AppDbContext db)
    : IRequestHandler<GetProductCategoriesQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(GetProductCategoriesQuery request, CancellationToken ct) =>
        await db.Products.AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.Low)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
}
