namespace MicroCommerce.Catalog.Application.Payouts.Dtos;

public record LedgerEntryDto(
    DateTimeOffset OccurredAt,
    string Label,
    string? Subject,
    decimal Amount,
    string Kind);

public record PayoutDto(
    Guid Id,
    DateTimeOffset SentAt,
    decimal Amount,
    string Period,
    string Destination,
    bool IsPending);

public record PayoutSummaryDto(
    decimal Available,
    decimal Pending,
    decimal NextSend,
    DateTimeOffset AvailableSendsOn,
    decimal Lifetime,
    int LifetimeOrders,
    string LifetimeRange);
