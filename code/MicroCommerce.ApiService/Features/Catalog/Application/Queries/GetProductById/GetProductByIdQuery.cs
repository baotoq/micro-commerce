using MediatR;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

