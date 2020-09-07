using System.Threading.Tasks;
using MassTransit;
using Ordering.API.IntegrationEvents.Models;

namespace Ordering.API.IntegrationEvents
{
    public class OrderCreatedConsumer : BaseConsumer<OrderCreated>
    {
        public override Task Consume(ConsumeContext<OrderCreated> context)
        {
            return Task.CompletedTask;
        }
    }
}
