using System.Threading.Tasks;
using Dapr;
using MediatR;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.IntegrationEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Controllers
{
    public class IntegrationEventController : BaseController
    {
        public IntegrationEventController(ILogger<IntegrationEventController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [Topic("pubsub", "product-deleted")]
        [HttpPost("product-deleted")]
        public async Task<IActionResult> Handle(ProductDeleted @event)
        {
            Logger.LogInformation("Hellpoooooo");
            
            await Task.Delay(10000);
            
            Logger.LogInformation("22222");

            return Ok();
        }

        [Topic("pubsub", "product-updated")]
        [HttpPost("product-updated")]
        public async Task<IActionResult> Handle(ProductUpdated @event)
        {
            Logger.LogInformation("Hellpoooooo");

            await Task.Delay(10000);

            Logger.LogInformation("22222");

            return Ok();
        }
    }
}
