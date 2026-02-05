using MediatR;

namespace MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCartItemCount;

public sealed record GetCartItemCountQuery(Guid BuyerId) : IRequest<int>;
