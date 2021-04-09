using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroCommerce.Shared.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Shared.EventBus
{
    public class DaprEventBus : IEventBus
    {
        private readonly string _pubsubName;

        private readonly DaprClient _dapr;
        private readonly ILogger<DaprEventBus> _logger;

        public DaprEventBus(string pubsubName, DaprClient dapr, ILogger<DaprEventBus> logger)
        {
            _pubsubName = pubsubName;
            _dapr = dapr;
            _logger = logger;
        }

        public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event, CancellationToken cancellationToken = default) where TIntegrationEvent : IntegrationEvent
        {
            var topicName = @event.GetType().Name;

            _logger.LogInformation("Publishing event {@Event} to {PubSubName}.{TopicName}", @event, _pubsubName, topicName);

            await _dapr.PublishEventAsync(_pubsubName, topicName, @event, cancellationToken);
        }
    }
}
