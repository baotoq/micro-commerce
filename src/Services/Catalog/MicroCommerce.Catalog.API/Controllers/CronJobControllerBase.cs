using System;
using System.Threading.Tasks;
using MediatR;
using MicroCommerce.Catalog.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Controllers
{
    public class CronJobController : ApiControllerBase
    {
        public CronJobController(ILogger<CronJobController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpPost("cron-test")]
        public Task Handle()
        {
            Logger.LogInformation($"Cron start @{DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
