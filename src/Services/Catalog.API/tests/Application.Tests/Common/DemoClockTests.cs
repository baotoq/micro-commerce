using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Infrastructure;

namespace MicroCommerce.Catalog.Application.Tests.Common;

public class DemoClockTests
{
    [Fact]
    public void Now_is_anchored_to_the_fixed_demo_instant()
    {
        IClock clock = new DemoClock();

        var expected = new DateTimeOffset(2026, 4, 8, 17, 45, 0, TimeSpan.FromHours(-7));

        Assert.Equal(expected, clock.Now);
    }

    [Fact]
    public void Now_equals_2026_04_09T00_45_00Z_in_utc()
    {
        var clock = new DemoClock();

        Assert.Equal(new DateTimeOffset(2026, 4, 9, 0, 45, 0, TimeSpan.Zero), clock.Now.ToUniversalTime());
    }

    [Fact]
    public void Now_is_deterministic_across_reads()
    {
        var clock = new DemoClock();

        Assert.Equal(clock.Now, clock.Now);
    }
}
