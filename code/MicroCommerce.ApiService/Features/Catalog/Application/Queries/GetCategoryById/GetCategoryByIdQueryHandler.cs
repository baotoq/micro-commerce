using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly CatalogDbContext _context;

    public GetCategoryByIdQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == new CategoryId(request.Id))
            .Select(c => new CategoryDto(
                c.Id.Value,
                c.Name.Value,
                c.Description,
                c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return category;
    }
}

