using System.Collections.Concurrent;
using MicroCommerce.ApiService.Features.Diagnostics;
using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Testing;
using Temporalio.Worker;

namespace MicroCommerce.UnitTests.Features.Diagnostics;

/// <summary>
/// Runs the real workflow on Temporal's time-skipping test server, so a long timer passes instantly.
/// The activities are stand-ins with the same names that record when and in what order they ran.
/// </summary>
public class SampleProcessWorkflowTests
{
    [Fact]
    public async Task Creates_then_waits_then_completes_then_publishes()
    {
        await using var env = await WorkflowEnvironment.StartTimeSkippingAsync();
        var activities = new RecordingActivities();
        var id = Guid.NewGuid();
        var delay = TimeSpan.FromMinutes(5);
        var startedAt = new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

        using var worker = new TemporalWorker(
            env.Client,
            new TemporalWorkerOptions($"test-{Guid.NewGuid()}")
                .AddAllActivities(activities)
                .AddWorkflow<SampleProcessWorkflow>());
        await worker.ExecuteAsync(() => env.Client.ExecuteWorkflowAsync(
            (SampleProcessWorkflow wf) => wf.RunAsync(id, delay, startedAt),
            new WorkflowOptions(SampleProcessWorkflow.WorkflowId(id), worker.Options.TaskQueue!)),
            TestContext.Current.CancellationToken);

        activities.Calls.Select(c => c.Name).ShouldBe(
            ["CreateSampleProcess", "CompleteSampleProcess", "PublishSampleProcessCompleted"]);
        activities.Calls.ShouldAllBe(c => c.Id == id);
        var created = activities.Calls[0].At;
        var completed = activities.Calls[1].At;
        (completed - created).ShouldBeGreaterThanOrEqualTo(delay);
        activities.PublishedCompletedAt.ShouldBe(RecordingActivities.CompletedAt);
    }

    private sealed class RecordingActivities
    {
        public static readonly DateTimeOffset CompletedAt = new(2026, 11, 5, 9, 5, 0, TimeSpan.Zero);

        private readonly ConcurrentQueue<(string Name, Guid Id, DateTime At)> _calls = new();

        public IReadOnlyList<(string Name, Guid Id, DateTime At)> Calls => [.. _calls];

        public DateTimeOffset? PublishedCompletedAt { get; private set; }

        [Activity("CreateSampleProcess")]
        public Task CreateAsync(Guid id, TimeSpan delay, DateTimeOffset startedAt) => Record("CreateSampleProcess", id);

        [Activity("CompleteSampleProcess")]
        public async Task<DateTimeOffset> CompleteAsync(Guid id)
        {
            await Record("CompleteSampleProcess", id);
            return CompletedAt;
        }

        [Activity("PublishSampleProcessCompleted")]
        public Task PublishAsync(Guid id, DateTimeOffset completedAt)
        {
            PublishedCompletedAt = completedAt;
            return Record("PublishSampleProcessCompleted", id);
        }

        // The scheduled time is the test server's (skipped) clock, so it shows the timer elapsed.
        private Task Record(string name, Guid id)
        {
            _calls.Enqueue((name, id, ActivityExecutionContext.Current.Info.ScheduledTime));
            return Task.CompletedTask;
        }
    }
}
