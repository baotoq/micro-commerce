using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Reviews.Domain.Events;

public sealed record ReviewUpdatedDomainEvent(Guid ReviewId, Guid ProductId) : DomainEvent;
