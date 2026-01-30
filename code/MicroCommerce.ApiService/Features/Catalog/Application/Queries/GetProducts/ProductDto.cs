namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCurrency,
    string? ImageUrl,
    string? Sku,
    string Status,
    Guid CategoryId,
    string CategoryName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

