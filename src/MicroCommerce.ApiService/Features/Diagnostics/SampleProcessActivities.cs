using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.ApiService.Infrastructure.Messaging;
using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;

namespace MicroCommerce.ApiService.Features.Diagnostics;

/// <summary>
/// The workflow's side effects. Temporal can run each activity more than once, so each is idempotent
/// (Principle V).
/// </summary>
public sealed class SampleProcessActivities(
    AppDbContext db,
    IIntegrationEventPublisher publisher,
    TimeProvider timeProvider,
    ILogger<SampleProcessActivities> logger)
{
    [Activity]
    public async Task CreateSampleProcessAsync(Guid id, TimeSpan delay, DateTimeOffset startedAt)
    {
        var processId = SampleProcessId.From(id);
        if (await db.SampleProcesses.AnyAsync(p => p.Id == processId, CancellationToken)) return;

        db.SampleProcesses.Add(SampleProcess.Start(processId, delay, startedAt));
        try
        {
            await db.SaveChangesAsync(CancellationToken);
            logger.LogInformation("Sample process {SampleProcessId} created", id);
        }
        catch (DbUpdateException e) when (e.IsUniqueViolation())
        {
            // A concurrent retry inserted it first.
        }
    }

    [Activity]
    public async Task<DateTimeOffset> CompleteSampleProcessAsync(Guid id)
    {
        var process = await db.SampleProcesses.SingleAsync(p => p.Id == SampleProcessId.From(id), CancellationToken);

        // A no-op when it already completed, so a retried activity keeps the first CompletedAt.
        process.Complete(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(CancellationToken);
        logger.LogInformation("Sample process {SampleProcessId} completed", id);

        return process.CompletedAt!.Value;
    }

    [Activity]
    public async Task PublishSampleProcessCompletedAsync(Guid id, DateTimeOffset completedAt)
    {
        var @event = new SampleProcessCompletedV1(SampleProcessCompletedV1.EventIdFor(id), id, completedAt);
        await publisher.PublishAsync(SampleProcessCompletedV1.Topic, @event, CancellationToken);
        logger.LogInformation("Sample process {SampleProcessId} completion published", id);
    }

    private static CancellationToken CancellationToken =>
        ActivityExecutionContext.HasCurrent ? ActivityExecutionContext.Current.CancellationToken : default;
}
