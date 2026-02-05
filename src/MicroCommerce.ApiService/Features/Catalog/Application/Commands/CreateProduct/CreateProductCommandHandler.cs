using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly CatalogDbContext _context;

    public CreateProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Verify category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == new CategoryId(request.CategoryId), cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {request.CategoryId} not found.");
        }

        var name = ProductName.Create(request.Name);
        var price = Money.Create(request.Price);

        var product = Product.Create(
            name,
            request.Description,
            price,
            new CategoryId(request.CategoryId),
            request.ImageUrl,
            request.Sku);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id.Value;
    }
}

