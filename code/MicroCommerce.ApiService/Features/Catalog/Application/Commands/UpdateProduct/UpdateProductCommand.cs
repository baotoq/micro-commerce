using MediatR;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    string? ImageUrl = null,
    string? Sku = null) : IRequest<bool>;

