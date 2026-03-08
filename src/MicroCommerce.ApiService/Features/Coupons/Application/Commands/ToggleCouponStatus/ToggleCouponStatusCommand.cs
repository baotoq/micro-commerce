using MediatR;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.ToggleCouponStatus;

public sealed record ToggleCouponStatusCommand(Guid Id, bool IsActive) : IRequest;
