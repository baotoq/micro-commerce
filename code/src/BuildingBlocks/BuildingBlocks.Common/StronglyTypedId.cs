using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MicroCommerce.BuildingBlocks.Common;

[DebuggerStepThrough]
public abstract record StronglyTypedId<T>(T Value)
{
    public override string ToString() => Value?.ToString() ?? string.Empty;
}
