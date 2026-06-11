using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Dtos;
using MicroCommerce.Catalog.Application.Payouts.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Payouts;

namespace MicroCommerce.Catalog.Application.Payouts.Commands;

// Internal-use command (no HTTP endpoint in v1 — see plan OQ6). Payouts are triggered by the
// system on a settlement schedule, not by seller UI actions.
public record CreatePayoutCommand(
    decimal Amount,
    string Period,
    string Destination) : IRequest<Result<PayoutDto>>;

public class CreatePayoutHandler(AppDbContext db, IPublisher publisher, IClock clock)
    : IRequestHandler<CreatePayoutCommand, Result<PayoutDto>>
{
    public async Task<Result<PayoutDto>> Handle(CreatePayoutCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return Result<PayoutDto>.Conflict("PAYOUT_AMOUNT_INVALID", "Payout amount must be positive.");

        var payout = new Payout(
            clock.Now,
            request.Amount,
            request.Period,
            request.Destination,
            isPending: false);

        db.Payouts.Add(payout);
        await db.SaveChangesAsync(ct);

        var dto = new PayoutDto(
            payout.Id.Value,
            payout.SentAt,
            payout.Amount,
            payout.Period,
            payout.Destination,
            payout.IsPending);

        await publisher.Publish(new PayoutCreatedEvent(dto.Id), ct);
        return Result<PayoutDto>.Success(dto);
    }
}
