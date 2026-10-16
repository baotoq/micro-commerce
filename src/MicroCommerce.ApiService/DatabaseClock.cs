namespace MicroCommerce.ApiService;

/// <summary>
/// The current time at the microsecond precision Postgres stores, so a timestamp an endpoint returns before
/// saving equals the one later read back. (.NET's clock ticks every 100 ns on Linux, every 1 µs on macOS.)
/// </summary>
public static class DatabaseClock
{
    public static DateTimeOffset UtcNow
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMicrosecond));
        }
    }
}
