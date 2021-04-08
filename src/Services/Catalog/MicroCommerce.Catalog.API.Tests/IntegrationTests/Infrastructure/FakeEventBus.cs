using System.Threading;
using System.Threading.Tasks;
using MicroCommerce.Shared.EventBus;
using MicroCommerce.Shared.EventBus.Abstractions;

namespace MicroCommerce.Catalog.API.Tests.IntegrationTests.Infrastructure
{
    public class FakeEventBus : IEventBus
    {
        public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event, CancellationToken cancellationToken = default) where TIntegrationEvent : IntegrationEvent
        {
            return Task.CompletedTask;
        }
    }
}
