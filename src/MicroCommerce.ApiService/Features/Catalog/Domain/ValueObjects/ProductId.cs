using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record ProductId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId From(Guid value) => new(value);
}

