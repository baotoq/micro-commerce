using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record DeleteProductCommand(string Sku) : IRequest<bool>;

public class DeleteProductHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        // AsTracking so the original xmin concurrency token (review finding 2) is captured for
        // the DELETE's WHERE clause; the global default is NoTracking, under which the shadow
        // token defaults to 0 and the concurrency-aware delete affects 0 rows -> throws.
        var product = await db.Products.AsTracking().FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (product is null) return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        await publisher.Publish(new ProductDeletedEvent(request.Sku), ct);
        return true;
    }
}
