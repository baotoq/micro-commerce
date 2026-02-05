using MediatR;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.UpdateCartItem;

public sealed record UpdateCartItemCommand(
    Guid BuyerId,
    Guid ItemId,
    int Quantity) : IRequest<Unit>;
