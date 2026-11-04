using FastEndpoints;
using MicroCommerce.ApiService.Infrastructure.Messaging;

namespace MicroCommerce.ApiService.Features.Diagnostics;

/// <summary>A Dapr CloudEvent envelope; only its data matters here.</summary>
public sealed record SampleProcessCompletedCloudEvent(SampleProcessCompletedV1? Data);

/// <summary>
/// Records that the completion event arrived. Duplicates are expected (at-least-once delivery) and
/// return 200 without changes; a failure returns 500 so that Dapr retries (FR-012b).
/// </summary>
public sealed class SampleProcessCompletedSubscriber(Inbox inbox, TimeProvider timeProvider)
    : Endpoint<SampleProcessCompletedCloudEvent>
{
    public const string Route = "/dapr/events/sample-process-completed";
    public const string Consumer = "sample-process-completed.record-receipt";

    public override void Configure()
    {
        Post(Route);
        RoutePrefixOverride(string.Empty);
        AuthSchemes(DaprSubscribeEndpoint.AuthScheme);
        Options(x => x.ExcludeFromDescription());
    }

    public override async Task HandleAsync(SampleProcessCompletedCloudEvent req, CancellationToken ct)
    {
        if (req.Data is null)
        {
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        await inbox.TryRecordAsync(Consumer, req.Data.EventId, timeProvider.GetUtcNow(), ct);
        await Send.OkAsync(ct);
    }
}
