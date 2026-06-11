using MediatR;

namespace MicroCommerce.Catalog.Application.Payouts.Events;

public record LedgerEntryCreatedEvent : INotification;

public record PayoutCreatedEvent(Guid Id) : INotification;
