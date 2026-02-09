using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

public sealed record UserProfileId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static UserProfileId New() => new(Guid.NewGuid());
}
