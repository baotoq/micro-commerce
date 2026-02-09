using MediatR;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(
    Guid BuyerId,
    Guid ItemId) : IRequest<Unit>;
