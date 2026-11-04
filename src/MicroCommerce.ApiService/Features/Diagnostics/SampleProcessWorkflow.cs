using Temporalio.Workflows;

namespace MicroCommerce.ApiService.Features.Diagnostics;

/// <summary>
/// Creates a sample process, waits for its delay on a durable timer, completes it and announces it.
/// The timer lives in Temporal, so the wait survives an API restart (FR-011, FR-012).
/// </summary>
[Workflow]
public sealed class SampleProcessWorkflow
{
    public const string TaskQueue = "diagnostics";

    private static readonly ActivityOptions Activity = new() { StartToCloseTimeout = TimeSpan.FromSeconds(30) };

    public static string WorkflowId(Guid id) => $"sample-process-{id}";

    [WorkflowRun]
    public async Task RunAsync(Guid id, TimeSpan delay, DateTimeOffset startedAt)
    {
        await Workflow.ExecuteActivityAsync((SampleProcessActivities a) => a.CreateSampleProcessAsync(id, delay, startedAt), Activity);
        await Workflow.DelayAsync(delay);
        var completedAt = await Workflow.ExecuteActivityAsync((SampleProcessActivities a) => a.CompleteSampleProcessAsync(id), Activity);
        await Workflow.ExecuteActivityAsync((SampleProcessActivities a) => a.PublishSampleProcessCompletedAsync(id, completedAt), Activity);
    }
}
