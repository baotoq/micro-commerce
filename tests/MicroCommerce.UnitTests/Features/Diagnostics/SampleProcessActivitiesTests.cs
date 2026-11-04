using MicroCommerce.ApiService.Features.Diagnostics;
using MicroCommerce.ApiService.Infrastructure.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.UnitTests.Features.Diagnostics;

public class SampleProcessActivitiesTests
{
    [Fact]
    public async Task Publishes_the_completion_with_a_deterministic_event_id()
    {
        var publisher = Substitute.For<IIntegrationEventPublisher>();
        // Publishing touches no database, so the activity gets none.
        var activities = new SampleProcessActivities(null!, publisher, TimeProvider.System, NullLogger<SampleProcessActivities>.Instance);
        var id = Guid.NewGuid();
        var completedAt = new DateTimeOffset(2026, 11, 5, 9, 0, 10, TimeSpan.Zero);

        await activities.PublishSampleProcessCompletedAsync(id, completedAt);

        await publisher.Received(1).PublishAsync(
            "sample-process-completed",
            new SampleProcessCompletedV1($"sample-process-completed:{id}", id, completedAt),
            Arg.Any<CancellationToken>());
    }
}
