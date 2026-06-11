using MediatR;

namespace MicroCommerce.Catalog.Application.Marketing.Events;

public record CampaignCreatedEvent(Guid Id) : INotification;

public record CampaignUpdatedEvent(Guid Id) : INotification;
