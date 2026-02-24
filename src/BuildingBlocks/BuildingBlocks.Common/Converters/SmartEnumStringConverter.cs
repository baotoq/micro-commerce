using Ardalis.SmartEnum;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroCommerce.BuildingBlocks.Common.Converters;

/// <summary>
/// Generic EF Core ValueConverter that stores a SmartEnum by its Name (string) in the database.
/// Stores by Name (not Value) to remain compatible with existing string-based database schemas.
/// </summary>
public class SmartEnumStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : SmartEnum<TEnum, int>
{
    public SmartEnumStringConverter()
        : base(
            smartEnum => smartEnum.Name,
            name => SmartEnum<TEnum, int>.FromName(name))
    {
    }
}
