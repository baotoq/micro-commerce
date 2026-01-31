using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateCategory;

/// <summary>
/// Handler for CreateCategoryCommand.
/// Creates a new category and persists it to the database.
/// </summary>
public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryId>
{
    private readonly CatalogDbContext _context;

    public CreateCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryId> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = CategoryName.Create(request.Name);
        var category = Category.Create(name, request.Description);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
