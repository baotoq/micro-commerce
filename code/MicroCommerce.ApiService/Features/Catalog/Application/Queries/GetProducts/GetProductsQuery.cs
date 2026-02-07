using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    string? Status = null,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = "asc") : IRequest<ProductListDto>;

