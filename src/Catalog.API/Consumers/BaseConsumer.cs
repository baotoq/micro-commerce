using System.Threading.Tasks;
using MassTransit;

namespace Catalog.API.Consumers
{
    public abstract class BaseMessage
    {
    }

    public abstract class BaseConsumer<T> : IConsumer<T> where T : BaseMessage
    {
        public virtual Task Consume(ConsumeContext<T> context)
        {
            return Task.CompletedTask;
        }
    }
}
