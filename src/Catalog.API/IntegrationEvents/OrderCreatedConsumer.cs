using System.Threading.Tasks;
using Catalog.API.IntegrationEvents.Models;
using MassTransit;

namespace Catalog.API.IntegrationEvents
{
    public class OrderCreatedConsumer : BaseConsumer<OrderCreated>
    {
        public override Task Consume(ConsumeContext<OrderCreated> context)
        {
            return Task.CompletedTask;
        }
    }
}
