using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

public sealed record AddressId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static AddressId New() => new(Guid.NewGuid());
}
