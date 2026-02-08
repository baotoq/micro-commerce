using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.ArchiveProduct;

public sealed class ArchiveProductCommandHandler
    : IRequestHandler<ArchiveProductCommand, bool>
{
    private readonly CatalogDbContext _context;

    public ArchiveProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        ArchiveProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == new ProductId(request.Id), cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        product.Archive();

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

