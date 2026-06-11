using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Dtos;
using MicroCommerce.Catalog.Application.Payouts.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Payouts;

namespace MicroCommerce.Catalog.Application.Payouts.Commands;

// Internal-use command (no HTTP endpoint in v1 — see plan OQ6). Ledger entries are written by
// future system triggers, not by seller UI actions.
public record CreateLedgerEntryCommand(
    DateTimeOffset OccurredAt,
    string Label,
    string? Subject,
    decimal Amount,
    string Kind,
    int? OrderNumber) : IRequest<Result<LedgerEntryDto>>;

public class CreateLedgerEntryHandler(AppDbContext db, IPublisher publisher)
    : IRequestHandler<CreateLedgerEntryCommand, Result<LedgerEntryDto>>
{
    public async Task<Result<LedgerEntryDto>> Handle(CreateLedgerEntryCommand request, CancellationToken ct)
    {
        var kind = ParseKind(request.Kind);

        var entry = new LedgerEntry(
            request.OccurredAt,
            request.Label,
            request.Subject,
            request.Amount,
            kind,
            request.OrderNumber);

        db.LedgerEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        var dto = new LedgerEntryDto(
            entry.OccurredAt,
            entry.Label,
            entry.Subject,
            entry.Amount,
            PayoutMapping.KindToString(entry.Kind));

        await publisher.Publish(new LedgerEntryCreatedEvent(), ct);
        return Result<LedgerEntryDto>.Success(dto);
    }

    private static LedgerEntryKind ParseKind(string s) => s.ToLowerInvariant() switch
    {
        "sale" => LedgerEntryKind.Sale,
        "fee" => LedgerEntryKind.Fee,
        "payout" => LedgerEntryKind.Payout,
        "label" => LedgerEntryKind.Label,
        _ => throw new ArgumentException($"Invalid ledger entry kind: {s}"),
    };
}
