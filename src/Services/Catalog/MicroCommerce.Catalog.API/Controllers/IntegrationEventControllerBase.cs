using System.Threading.Tasks;
using Dapr;
using MediatR;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.IntegrationEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Controllers
{
    [Route("events")]
    public class IntegrationEventController : ApiControllerBase
    {
        public const string PubsubName = "pubsub";
        
        public IntegrationEventController(ILogger<IntegrationEventController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [Topic(PubsubName, nameof(ProductDeleted))]
        [HttpPost(nameof(ProductDeleted))]
        public async Task Handle(ProductDeleted @event)
        {
            await Task.Delay(1000);
            await Mediator.Send(@event);
        }

        [Topic(PubsubName, nameof(ProductUpdated))]
        [HttpPost(nameof(ProductUpdated))]
        public async Task Handle(ProductUpdated @event)
        {
            await Task.Delay(1000);
            await Mediator.Send(@event);
        }
    }
}
