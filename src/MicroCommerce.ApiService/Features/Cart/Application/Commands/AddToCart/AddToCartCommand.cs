using MediatR;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.AddToCart;

public sealed record AddToCartCommand(
    Guid BuyerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    int Quantity = 1) : IRequest<AddToCartResult>;

public sealed record AddToCartResult(bool IsUpdate);
