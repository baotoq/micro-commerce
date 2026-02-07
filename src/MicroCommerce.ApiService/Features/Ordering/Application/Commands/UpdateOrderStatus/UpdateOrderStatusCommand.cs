using MediatR;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    string NewStatus) : IRequest;
