using MediatR;
using MicroCommerce.Application.Products.Dtos;
using MicroCommerce.Domain.Products;
using MicroCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Application.Products.Commands;

public record CreateProductCommand(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : IRequest<ProductDto>;

public class CreateProductHandler(AppDbContext db) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);

        if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
            throw new InvalidOperationException($"Product with SKU '{request.Sku}' already exists.");

        var product = new Product(sku, request.Name, request.Category, request.Price, request.Inventory, ParseStatus(request.Status));
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    internal static ProductDto ToDto(Product p) =>
        new(p.Sku.Value, p.Name, p.Category, p.Price, p.Inventory, p.Status.ToString().ToLowerInvariant(), p.Views7d);

    internal static ProductStatus ParseStatus(string s) => s.ToLowerInvariant() switch
    {
        "active" => ProductStatus.Active,
        "low" => ProductStatus.Low,
        "out" => ProductStatus.Out,
        "draft" => ProductStatus.Draft,
        _ => throw new ArgumentException($"Invalid status: {s}"),
    };
}
