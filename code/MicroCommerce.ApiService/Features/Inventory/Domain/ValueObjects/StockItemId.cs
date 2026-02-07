using System.Diagnostics;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

[DebuggerStepThrough]
public sealed record StockItemId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static StockItemId New() => new(Guid.NewGuid());
    public static StockItemId From(Guid value) => new(value);
}
