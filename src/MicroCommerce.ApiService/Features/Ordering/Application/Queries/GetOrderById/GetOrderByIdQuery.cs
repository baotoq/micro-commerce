using MediatR;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
