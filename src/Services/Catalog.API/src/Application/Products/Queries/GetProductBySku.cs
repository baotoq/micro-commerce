using MediatR;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductBySkuQuery(string Sku) : IRequest<ProductDto?>;

public class GetProductBySkuHandler(AppDbContext db) : IRequestHandler<GetProductBySkuQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductBySkuQuery request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);
        return product is null ? null : CreateProductHandler.ToDto(product);
    }
}
