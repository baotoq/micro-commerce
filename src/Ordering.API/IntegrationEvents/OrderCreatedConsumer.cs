using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Ordering.API.Application.Orders.Commands;
using Ordering.API.IntegrationEvents.Models;

namespace Ordering.API.IntegrationEvents
{
    public class OrderCreatedConsumer : BaseConsumer<OrderCreated>
    {
        private readonly IMediator _mediator;
        private readonly IBus _bus;

        public OrderCreatedConsumer(IMediator mediator, IBus bus)
        {
            _mediator = mediator;
            _bus = bus;
        }

        public override async Task Consume(ConsumeContext<OrderCreated> context)
        {
            
        }
    }
}
