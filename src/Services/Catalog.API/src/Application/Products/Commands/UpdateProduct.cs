using MediatR;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record UpdateProductCommand(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : IRequest<ProductDto?>;

public class UpdateProductHandler(AppDbContext db) : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (product is null) return null;

        product.Update(request.Name, request.Category, request.Price, request.Inventory, CreateProductHandler.ParseStatus(request.Status));
        await db.SaveChangesAsync(ct);
        return CreateProductHandler.ToDto(product);
    }
}
