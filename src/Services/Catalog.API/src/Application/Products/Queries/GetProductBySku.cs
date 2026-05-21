using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductBySkuQuery(string Sku) : IRequest<ProductDto?>;

public class GetProductBySkuHandler(AppDbContext db) : IRequestHandler<GetProductBySkuQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductBySkuQuery request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        return await db.Products
            .AsNoTracking()
            .Where(p => p.Sku == sku)
            .Select(p => new ProductDto(
                p.Sku.Value,
                p.Name,
                p.Category,
                p.Price,
                p.Inventory,
                p.Status == ProductStatus.Active ? "active"
                    : p.Status == ProductStatus.Low ? "low"
                    : p.Status == ProductStatus.Out ? "out"
                    : "draft",
                p.Views7d))
            .FirstOrDefaultAsync(ct);
    }
}
