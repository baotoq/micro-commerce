using MediatR;

namespace MicroCommerce.Catalog.Application.Promotions.Events;

public record PromotionCreatedEvent(string Code, string Status) : INotification;

public record PromotionUpdatedEvent(string Code, string Status) : INotification;

public record PromotionDeletedEvent(string Code) : INotification;

public record PromotionStatusChangedEvent(string Code, string Status) : INotification;
