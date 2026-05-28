using MediatR;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Commands;

public record UpdateProductCommand(
    string Sku,
    string Name,
    string Category,
    decimal Price,
    int Inventory,
    string Status,
    string? Description = null,
    IReadOnlyList<string>? Tags = null,
    decimal Weight = 0.01m,
    string Origin = "Unknown, NA",
    IReadOnlyList<string>? PhotoUrls = null) : IRequest<ProductDto?>;

public class UpdateProductHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var sku = Sku.From(request.Sku);
        // The DbContext default is QueryTrackingBehavior.NoTracking (set in
        // InfrastructureExtensions), which keeps reads cheap but means
        // mutations on the returned entity wouldn't persist. Opt-in tracking
        // here so product.Update() flows through to SaveChangesAsync.
        var product = await db.Products
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (product is null) return null;

        // PATCH-style: nullable wire fields preserve the stored value when the
        // caller omits them (legacy EditListingForm only submits six fields).
        // Sending an explicit empty list still wipes the stored value.
        product.Update(
            request.Name,
            request.Category,
            request.Price,
            request.Inventory,
            CreateProductHandler.ParseStatus(request.Status),
            description: request.Description ?? product.Description,
            tags: request.Tags ?? product.Tags,
            weight: request.Weight,
            origin: request.Origin,
            photoUrls: request.PhotoUrls ?? product.PhotoUrls);
        await db.SaveChangesAsync(ct);

        var dto = CreateProductHandler.ToDto(product);
        await publisher.Publish(new ProductUpdatedEvent(dto.Sku, dto.Name, dto.Category, dto.Price, dto.Inventory, dto.Status), ct);
        return dto;
    }
}
