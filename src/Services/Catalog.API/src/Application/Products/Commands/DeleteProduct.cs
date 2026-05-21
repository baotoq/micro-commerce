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
        var product = await db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (product is null) return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        await publisher.Publish(new ProductDeletedEvent(request.Sku), ct);
        return true;
    }
}
