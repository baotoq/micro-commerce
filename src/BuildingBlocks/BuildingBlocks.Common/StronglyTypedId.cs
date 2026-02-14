using System.Diagnostics;

namespace MicroCommerce.BuildingBlocks.Common;

[DebuggerStepThrough]
public abstract record StronglyTypedId<T>(T Value) : IComparable<StronglyTypedId<T>>
    where T : notnull, IComparable<T>
{
    public int CompareTo(StronglyTypedId<T>? other)
    {
        return other is null ? 1 : Value.CompareTo(other.Value);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
