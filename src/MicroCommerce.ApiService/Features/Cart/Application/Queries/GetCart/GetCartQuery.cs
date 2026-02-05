using MediatR;

namespace MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCart;

public sealed record GetCartQuery(Guid BuyerId) : IRequest<CartDto?>;
