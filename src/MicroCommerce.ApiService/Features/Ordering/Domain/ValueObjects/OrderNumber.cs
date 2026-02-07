using System.Diagnostics;
using System.Security.Cryptography;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

/// <summary>
/// Order number in MC-XXXXXX format using unambiguous alphanumeric characters.
/// Excludes 0/O/1/I/L to avoid confusion in print and display.
/// </summary>
[DebuggerStepThrough]
public sealed record OrderNumber
{
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;

    public string Value { get; }

    private OrderNumber(string value)
    {
        Value = value;
    }

    public static OrderNumber Generate()
    {
        Span<char> code = stackalloc char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            code[i] = AllowedChars[RandomNumberGenerator.GetInt32(AllowedChars.Length)];
        }

        return new OrderNumber($"MC-{new string(code)}");
    }

    public static OrderNumber From(string value) => new(value);

    public override string ToString() => Value;
}
