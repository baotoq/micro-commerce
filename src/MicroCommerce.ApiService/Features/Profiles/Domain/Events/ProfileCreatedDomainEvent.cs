using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.Events;

public sealed record ProfileCreatedDomainEvent(UserProfileId ProfileId, Guid UserId) : DomainEvent;
