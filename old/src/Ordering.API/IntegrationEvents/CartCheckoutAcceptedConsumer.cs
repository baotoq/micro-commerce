using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Ordering.API.Application.Orders.Commands;
using Ordering.API.IntegrationEvents.Models;

namespace Ordering.API.IntegrationEvents
{
    public class CartCheckoutAcceptedConsumer : BaseConsumer<CartCheckoutAccepted>
    {
        private readonly IMediator _mediator;
        private readonly IBus _bus;

        public CartCheckoutAcceptedConsumer(IMediator mediator, IBus bus)
        {
            _mediator = mediator;
            _bus = bus;
        }

        public override async Task Consume(ConsumeContext<CartCheckoutAccepted> context)
        {
            await _mediator.Send(new CreateOrderCommand
            {
                CartId = context.Message.CartId
            });

            await _bus.Publish(new OrderCreated
            {
                OrderId = 1
            }, context.CancellationToken);
        }
    }
}
