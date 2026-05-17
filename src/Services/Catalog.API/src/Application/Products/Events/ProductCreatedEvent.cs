using MediatR;

namespace MicroCommerce.Catalog.Application.Products.Events;

public record ProductCreatedEvent(
    string Sku,
    string Name,
    string Category,
    decimal Price,
    int Inventory,
    string Status) : INotification;
