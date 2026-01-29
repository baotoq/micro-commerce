using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;

/// <summary>
/// Handler for GetCategoriesQuery.
/// Retrieves all categories from the database.
/// </summary>
public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly CatalogDbContext _context;

    public GetCategoriesQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name.Value)
            .Select(c => new CategoryDto(
                c.Id.Value,
                c.Name.Value,
                c.Description,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
