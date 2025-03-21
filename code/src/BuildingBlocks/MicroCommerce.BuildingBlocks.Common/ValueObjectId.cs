using System.Diagnostics.CodeAnalysis;

namespace MicroCommerce.BuildingBlocks.Common;

public record ValueObjectId(string Value)
{
}

public record ValueObjectId<T>(string Value) : ValueObjectId(Value)
{
    [return: NotNullIfNotNull(nameof(value))]
    public static implicit operator ValueObjectId<T>?(string? value) => value != null ? new(value) : null;

    public static explicit operator string(ValueObjectId<T> value) => value.Value;
}

public record ValueGuidObjectId(Guid Value)
{
}

public record ValueGuidObjectId<T>(Guid Value) : ValueGuidObjectId(Value)
{
    [return: NotNullIfNotNull(nameof(value))]
    public static implicit operator ValueGuidObjectId<T>?(string? value) => value != null ? new(value) : null;

    public static explicit operator string(ValueGuidObjectId<T> value) => value.ToString();
}
