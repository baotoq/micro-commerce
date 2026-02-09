using MediatR;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.MergeCarts;

public sealed record MergeCartsCommand(Guid GuestBuyerId, Guid AuthenticatedBuyerId) : IRequest;
