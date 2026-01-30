using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly CatalogDbContext _context;

    public UpdateCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == new CategoryId(request.Id), cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found.");
        }

        var name = CategoryName.Create(request.Name);
        category.Update(name, request.Description);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

