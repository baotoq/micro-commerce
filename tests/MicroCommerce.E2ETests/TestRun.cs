using System.Security.Cryptography;

namespace MicroCommerce.E2ETests;

/// <summary>
/// Values unique to this test run (data-model "Test run"). Every name a test creates includes the
/// run suffix, so tests never mistake another test's or another run's data for their own (FR-015).
/// </summary>
public static class TestRun
{
    private static int _n;

    /// <summary>6 lowercase hex characters, fixed for the whole run.</summary>
    public static string Suffix { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToLowerInvariant();

    /// <summary>A name unique within the run, for example <c>Sunny Mugs 3f9a1c-4</c>.</summary>
    public static string Unique(string prefix) => $"{prefix} {Suffix}-{Interlocked.Increment(ref _n)}";

    /// <summary>The next number in the run, for names that cannot contain spaces.</summary>
    public static int Next() => Interlocked.Increment(ref _n);
}
