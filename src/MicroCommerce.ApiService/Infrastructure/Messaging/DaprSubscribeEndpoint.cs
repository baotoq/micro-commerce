using FastEndpoints;
using MicroCommerce.ApiService.Features.Diagnostics;

namespace MicroCommerce.ApiService.Infrastructure.Messaging;

public sealed record DaprSubscription(string Pubsubname, string Topic, string Route);

/// <summary>
/// Dapr's programmatic subscription list. Called by the sidecar, authenticated with the app API
/// token, outside the /api prefix and the OpenAPI document.
/// </summary>
public sealed class DaprSubscribeEndpoint : EndpointWithoutRequest<IReadOnlyList<DaprSubscription>>
{
    public const string AuthScheme = "Dapr";

    public override void Configure()
    {
        Get("/dapr/subscribe");
        RoutePrefixOverride(string.Empty);
        AuthSchemes(AuthScheme);
        Options(x => x.ExcludeFromDescription());
    }

    public override Task HandleAsync(CancellationToken ct) =>
        Send.OkAsync(
        [
            new DaprSubscription(DaprIntegrationEventPublisher.PubSubName, SampleProcessCompletedV1.Topic, SampleProcessCompletedSubscriber.Route),
        ],
        ct);
}
