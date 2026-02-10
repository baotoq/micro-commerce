using MediatR;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.SimulatePayment;

public sealed record SimulatePaymentCommand(
    Guid OrderId,
    bool ShouldSucceed) : IRequest<SimulatePaymentResult>;

public sealed record SimulatePaymentResult(bool Success, string? FailureReason);
