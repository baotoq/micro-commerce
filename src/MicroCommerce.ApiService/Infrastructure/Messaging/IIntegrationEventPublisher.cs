namespace MicroCommerce.ApiService.Infrastructure.Messaging;

/// <summary>Publishes integration events to other areas. Delivery is at least once.</summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(string topic, T @event, CancellationToken ct);
}
