using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Wishlists.Domain.ValueObjects;

public sealed record WishlistItemId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static WishlistItemId New() => new(Guid.NewGuid());
}
