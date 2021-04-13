using System.Threading;
using System.Threading.Tasks;
using MicroCommerce.Shared.EventBus.Models;

namespace MicroCommerce.Shared.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event, CancellationToken cancellationToken = default) where TIntegrationEvent : IntegrationEvent;
    }
}
