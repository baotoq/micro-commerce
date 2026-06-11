using MediatR;

namespace MicroCommerce.Catalog.Application.Orders.Events;

public record OrderCreatedEvent(int Number, string Status) : INotification;

public record OrderStatusChangedEvent(int Number, string Status) : INotification;

public record OrderUpdatedEvent(int Number) : INotification;
