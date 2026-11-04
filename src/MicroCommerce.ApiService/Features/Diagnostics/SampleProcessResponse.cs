namespace MicroCommerce.ApiService.Features.Diagnostics;

public sealed record SampleProcessResponse(
    Guid Id,
    string Status,
    int DelaySeconds,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CompletionEventReceivedAt);
