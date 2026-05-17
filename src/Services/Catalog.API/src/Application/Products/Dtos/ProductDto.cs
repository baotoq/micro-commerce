namespace MicroCommerce.Catalog.Application.Products.Dtos;

public record ProductDto(string Sku, string Name, string Category, decimal Price, int Inventory, string Status, int Views7d);
public record ProductCountsDto(int Total, int Active, int Low, int Out, int Draft);
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
