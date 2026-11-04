using FastEndpoints;
using Mapster;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Diagnostics;

public sealed record GetSampleProcessRequest(Guid Id);

public sealed class GetSampleProcessEndpoint(AppDbContext db, TypeAdapterConfig mapping)
    : Endpoint<GetSampleProcessRequest, SampleProcessResponse>
{
    public override void Configure()
    {
        Get("/diagnostics/sample-processes/{id}");
        Policies(AuthPolicies.PlatformOperator);
        Summary(s =>
        {
            s.Summary = "Get a sample durable process";
            s.Responses[200] = "The process, with its completion and the receipt of its completion event.";
            s.Responses[400] = "The ID isn't a UUID.";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "Not a Platform Operator.";
            s.Responses[404] = "No such process, or the workflow hasn't created it yet.";
        });
    }

    public override async Task HandleAsync(GetSampleProcessRequest req, CancellationToken ct)
    {
        var id = SampleProcessId.From(req.Id);
        var process = await db.SampleProcesses.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
        if (process is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var messageId = SampleProcessCompletedV1.EventIdFor(req.Id);
        var receivedAt = await db.ProcessedEvents.AsNoTracking()
            .Where(e => e.Consumer == SampleProcessCompletedSubscriber.Consumer && e.MessageId == messageId)
            .Select(e => (DateTimeOffset?)e.ProcessedAt)
            .SingleOrDefaultAsync(ct);

        var response = process.Adapt<SampleProcessResponse>(mapping) with { CompletionEventReceivedAt = receivedAt };
        await Send.OkAsync(response, ct);
    }
}
