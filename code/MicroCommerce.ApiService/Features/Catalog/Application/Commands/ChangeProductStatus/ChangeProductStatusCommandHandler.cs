using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusCommandHandler
    : IRequestHandler<ChangeProductStatusCommand, bool>
{
    private readonly CatalogDbContext _context;

    public ChangeProductStatusCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        ChangeProductStatusCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == new ProductId(request.Id), cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        if (request.Status.Equals("Published", StringComparison.OrdinalIgnoreCase))
        {
            product.Publish();
        }
        else
        {
            product.Unpublish();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

