using NSubstitute;
using RedLockNet;

namespace MicroCommerce.Tests;

public class TestHelper
{
    public static IDistributedLockFactory CreateAcquiredLock()
    {
        var distributedLockFactory = Substitute.For<IDistributedLockFactory>();
        var redLock = Substitute.For<IRedLock>();

        redLock.IsAcquired.Returns(true);
        distributedLockFactory
            .CreateLockAsync(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(redLock);
        return distributedLockFactory;
    }
}
