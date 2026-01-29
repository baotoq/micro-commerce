using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record CategoryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CategoryId New() => new(Guid.NewGuid());
    public static CategoryId From(Guid value) => new(value);
}
