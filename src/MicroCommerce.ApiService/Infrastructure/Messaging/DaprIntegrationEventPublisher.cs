using Dapr.Client;

namespace MicroCommerce.ApiService.Infrastructure.Messaging;

public sealed class DaprIntegrationEventPublisher(DaprClient dapr) : IIntegrationEventPublisher
{
    public const string PubSubName = "pubsub";

    public Task PublishAsync<T>(string topic, T @event, CancellationToken ct) =>
        dapr.PublishEventAsync(PubSubName, topic, @event, ct);
}
