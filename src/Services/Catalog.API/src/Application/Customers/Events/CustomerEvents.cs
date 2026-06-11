using MediatR;

namespace MicroCommerce.Catalog.Application.Customers.Events;

public record CustomerCreatedEvent(string Email) : INotification;

public record CustomerUpdatedEvent(string Email) : INotification;
