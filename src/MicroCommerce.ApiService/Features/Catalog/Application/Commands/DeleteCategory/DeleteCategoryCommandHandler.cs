using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly CatalogDbContext _context;

    public DeleteCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == new CategoryId(request.Id), cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found.");
        }

        // Check if any products use this category
        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == new CategoryId(request.Id), cancellationToken);

        if (hasProducts)
        {
            throw new ConflictException("Cannot delete category that has products. Remove or reassign products first.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

