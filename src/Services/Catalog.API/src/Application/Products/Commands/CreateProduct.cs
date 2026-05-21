using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record CreateProductCommand(string Sku, string Name, string Category, decimal Price, int Inventory, string Status) : IRequest<Result<ProductDto>>;

public class CreateProductHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private const string PostgresUniqueViolation = "23505";

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);

        // Cheap optimistic pre-check. Not race-safe on its own — the unique index on Sku is the
        // authoritative guarantee, enforced via the DbUpdateException catch below (audit#3).
        if (await db.Products.AsNoTracking().AnyAsync(p => p.Sku == sku, ct))
            return Result<ProductDto>.Conflict("PRODUCT_SKU_DUPLICATE", $"Product with SKU '{request.Sku}' already exists.");

        var product = new Product(sku, request.Name, request.Category, request.Price, request.Inventory, ParseStatus(request.Status));
        db.Products.Add(product);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation })
        {
            return Result<ProductDto>.Conflict("PRODUCT_SKU_DUPLICATE", $"Product with SKU '{request.Sku}' already exists.");
        }

        var dto = ToDto(product);
        await publisher.Publish(new ProductCreatedEvent(dto.Sku, dto.Name, dto.Category, dto.Price, dto.Inventory, dto.Status), ct);
        return Result<ProductDto>.Success(dto);
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
