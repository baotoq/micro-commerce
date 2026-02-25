using FluentResults;
using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusCommandHandler(CatalogDbContext context)
    : IRequestHandler<ChangeProductStatusCommand, Result>
{
    public async Task<Result> Handle(
        ChangeProductStatusCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Product? product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == ProductId.From(request.Id), cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        try
        {
            switch (request.Status.ToLowerInvariant())
            {
                case "published":
                    product.Publish();
                    break;
                case "draft":
                    product.Unpublish();
                    break;
                case "archived":
                    product.Archive();
                    break;
                default:
                    return Result.Fail($"Unknown status: {request.Status}");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
