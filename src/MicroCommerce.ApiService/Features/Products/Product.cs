using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Products;

public class Product
{
    public ProductId Id { get; init; } = ProductId.New();
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);
    }
}

public record ProductResponse(ProductId Id, string Name, decimal Price, DateTimeOffset CreatedAt)
{
    public static ProductResponse From(Product p) => new(p.Id, p.Name, p.Price, p.CreatedAt);
}
