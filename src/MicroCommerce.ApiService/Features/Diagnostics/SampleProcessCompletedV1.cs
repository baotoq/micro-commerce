namespace MicroCommerce.ApiService.Features.Diagnostics;

/// <summary>Integration event: a sample process completed (contracts/events.md).</summary>
public sealed record SampleProcessCompletedV1(string EventId, Guid SampleProcessId, DateTimeOffset CompletedAt)
{
    public const string Topic = "sample-process-completed";

    /// <summary>Deterministic per process, so a retried publish carries the same ID.</summary>
    public static string EventIdFor(Guid sampleProcessId) => $"sample-process-completed:{sampleProcessId}";
}
