using System.Threading;
using System.Threading.Tasks;
using MicroCommerce.Shared.EventBus.Abstractions;
using MicroCommerce.Shared.EventBus.Models;

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
