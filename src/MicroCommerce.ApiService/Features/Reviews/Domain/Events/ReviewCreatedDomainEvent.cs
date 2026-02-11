using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Reviews.Domain.Events;

public sealed record ReviewCreatedDomainEvent(Guid ReviewId, Guid ProductId) : DomainEvent;
