using MediatR;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReleaseReservation;

public sealed record ReleaseReservationCommand(
    Guid StockItemId,
    Guid ReservationId) : IRequest<Unit>;
