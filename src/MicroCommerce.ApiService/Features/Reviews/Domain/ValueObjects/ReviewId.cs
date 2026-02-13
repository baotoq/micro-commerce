using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;

public sealed record ReviewId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ReviewId New() => new(Guid.NewGuid());
}
