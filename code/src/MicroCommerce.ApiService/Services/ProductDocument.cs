using MicroCommerce.ApiService.Domain.Entities;

namespace MicroCommerce.ApiService.Services;

public record ProductDocument
{
    public const string IndexPattern = "*product*";

    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }

    public static ProductDocument FromDomain(Product product)
    {
        return new ProductDocument
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}
