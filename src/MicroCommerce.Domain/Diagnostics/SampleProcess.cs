using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Diagnostics;

/// <summary>
/// A technical demonstration of a durable, time-based process: it waits for its delay, then completes
/// exactly once. Not a Marketplace concept; a real lifecycle process replaces it.
/// </summary>
public sealed class SampleProcess : AggregateRoot<SampleProcessId>
{
    private static readonly TimeSpan MinDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(300);

    private SampleProcess()
    {
    }

    public TimeSpan Delay { get; private set; }

    public SampleProcessStatus Status { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset DueAt => StartedAt + Delay;

    public static SampleProcess Start(SampleProcessId id, TimeSpan delay, DateTimeOffset now)
    {
        if (delay < MinDelay || delay > MaxDelay)
            throw new DomainRuleViolation("A sample process must wait between 1 and 300 seconds");

        return new SampleProcess
        {
            Id = id,
            Delay = delay,
            Status = SampleProcessStatus.Running,
            StartedAt = now,
        };
    }

    /// <summary>Completes the process once its delay has passed. Completing it again changes nothing.</summary>
    public void Complete(DateTimeOffset now)
    {
        if (Status == SampleProcessStatus.Completed) return;

        if (now < DueAt)
            throw new DomainRuleViolation("A sample process can't complete before its delay has passed");

        Status = SampleProcessStatus.Completed;
        CompletedAt = now;
        RaiseDomainEvent(new SampleProcessCompleted(Id, now));
    }
}
