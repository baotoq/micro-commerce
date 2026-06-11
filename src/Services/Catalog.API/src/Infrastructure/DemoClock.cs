using MicroCommerce.Catalog.Domain.Common;

namespace MicroCommerce.Catalog.Infrastructure;

/// <summary>
/// Fixed demo clock anchored to 2026-04-08T17:45:00-07:00 (== 2026-04-09T00:45:00Z, civil
/// date Apr 8 in America/Los_Angeles). This is the single shared demo "now": the frontend
/// <c>DEMO_NOW</c> uses the identical instant (2026-04-09T00:45:00Z). Swap to a SystemClock
/// when real buyer sessions exist.
/// </summary>
public sealed class DemoClock : IClock
{
    public DateTimeOffset Now => new DateTimeOffset(2026, 4, 8, 17, 45, 0, TimeSpan.FromHours(-7));
}
