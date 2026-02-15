using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record CartId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CartId New() => new(Guid.CreateVersion7());
    public static CartId From(Guid value) => new(value);
}
