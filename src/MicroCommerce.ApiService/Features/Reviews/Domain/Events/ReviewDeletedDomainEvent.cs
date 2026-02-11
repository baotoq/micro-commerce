using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Reviews.Domain.Events;

public sealed record ReviewDeletedDomainEvent(Guid ReviewId, Guid ProductId) : DomainEvent;
