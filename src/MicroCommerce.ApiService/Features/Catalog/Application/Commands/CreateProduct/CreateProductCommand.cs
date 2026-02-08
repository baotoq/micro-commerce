using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    string? ImageUrl = null,
    string? Sku = null) : IRequest<Guid>;

