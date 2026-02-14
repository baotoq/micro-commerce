using System.Reflection;

namespace MicroCommerce.BuildingBlocks.Common;

public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>, IComparable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<Dictionary<int, TEnum>> _byValue = new(
        () => GetAll().ToDictionary(e => e.Value));

    private static readonly Lazy<Dictionary<string, TEnum>> _byName = new(
        () => GetAll().ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase));

    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; }
    public string Name { get; }

    public static IReadOnlyCollection<TEnum> GetAll()
    {
        return typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(TEnum))
            .Select(f => (TEnum)f.GetValue(null)!)
            .ToList()
            .AsReadOnly();
    }

    public static TEnum FromValue(int value)
    {
        if (_byValue.Value.TryGetValue(value, out TEnum? result))
            return result;

        throw new InvalidOperationException($"'{value}' is not a valid value for {typeof(TEnum).Name}.");
    }

    public static TEnum FromName(string name)
    {
        if (_byName.Value.TryGetValue(name, out TEnum? result))
            return result;

        throw new InvalidOperationException($"'{name}' is not a valid name for {typeof(TEnum).Name}.");
    }

    public static bool TryFromValue(int value, out TEnum? result)
    {
        return _byValue.Value.TryGetValue(value, out result);
    }

    public static bool TryFromName(string name, out TEnum? result)
    {
        return _byName.Value.TryGetValue(name, out result);
    }

    public int CompareTo(Enumeration<TEnum>? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public bool Equals(Enumeration<TEnum>? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Enumeration<TEnum> other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Name;

    public static bool operator ==(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration<TEnum>? left, Enumeration<TEnum>? right) => !(left == right);
}
