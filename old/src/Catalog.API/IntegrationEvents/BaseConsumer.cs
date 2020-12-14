using System.Threading.Tasks;
using Catalog.API.IntegrationEvents.Models;
using MassTransit;

namespace Catalog.API.IntegrationEvents
{
    public abstract class BaseConsumer<T> : IConsumer<T> where T : BaseMessage
    {
        public abstract Task Consume(ConsumeContext<T> context);
    }
}
