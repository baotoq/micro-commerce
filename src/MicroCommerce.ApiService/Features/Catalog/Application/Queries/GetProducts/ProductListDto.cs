namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;

public sealed record ProductListDto(
    IReadOnlyList<ProductDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

