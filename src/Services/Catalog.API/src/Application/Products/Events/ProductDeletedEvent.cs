using MediatR;

namespace MicroCommerce.Catalog.Application.Products.Events;

public record ProductDeletedEvent(string Sku) : INotification;
