using System.Threading.Tasks;
using MassTransit;
using Ordering.API.IntegrationEvents.Models;

namespace Ordering.API.IntegrationEvents
{
    public abstract class BaseConsumer<T> : IConsumer<T> where T : BaseMessage
    {
        public abstract Task Consume(ConsumeContext<T> context);
    }
}
