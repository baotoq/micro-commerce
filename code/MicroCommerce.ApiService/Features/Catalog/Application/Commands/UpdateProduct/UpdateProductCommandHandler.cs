using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly CatalogDbContext _context;

    public UpdateProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == new ProductId(request.Id), cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        // Verify category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == new CategoryId(request.CategoryId), cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {request.CategoryId} not found.");
        }

        var name = ProductName.Create(request.Name);
        var price = Money.Create(request.Price);

        product.Update(
            name,
            request.Description,
            price,
            new CategoryId(request.CategoryId),
            request.ImageUrl,
            request.Sku);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

