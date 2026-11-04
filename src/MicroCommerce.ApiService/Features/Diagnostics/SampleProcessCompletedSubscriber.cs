using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Infrastructure.Messaging;

namespace MicroCommerce.ApiService.Features.Diagnostics;

/// <summary>A Dapr CloudEvent envelope; only its data matters here.</summary>
public sealed record SampleProcessCompletedCloudEvent(SampleProcessCompletedV1? Data);

/// <summary>The event comes from outside the process, so it is validated at the boundary.</summary>
public sealed class SampleProcessCompletedCloudEventValidator : Validator<SampleProcessCompletedCloudEvent>
{
    public SampleProcessCompletedCloudEventValidator()
    {
        RuleFor(x => x.Data).NotNull().WithMessage("data is required.");
        When(x => x.Data is not null, () =>
        {
            RuleFor(x => x.Data!.EventId).NotEmpty().WithMessage("data.eventId is required.");
            RuleFor(x => x.Data!.SampleProcessId).NotEmpty().WithMessage("data.sampleProcessId is required.");
        });
    }
}

/// <summary>
/// Records that the completion event arrived. Duplicates are expected (at-least-once delivery) and
/// return 200 without changes; a failure returns 500 so that Dapr retries (FR-012b).
/// </summary>
public sealed class SampleProcessCompletedSubscriber(IInbox inbox, TimeProvider timeProvider)
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
        // The validator guarantees Data.
        await inbox.TryRecordAsync(Consumer, req.Data!.EventId, timeProvider.GetUtcNow(), ct);
        await Send.OkAsync(ct);
    }
}
