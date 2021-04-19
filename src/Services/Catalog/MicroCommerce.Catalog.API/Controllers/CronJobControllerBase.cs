using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Controllers
{
    public class CronJobController : ApiControllerBase
    {
        private readonly DaprClient _daprClient;

        public CronJobController(ILogger<CronJobController> logger, IMediator mediator, DaprClient daprClient) : base(logger, mediator)
        {
            _daprClient = daprClient;
        }

        [HttpPost("cron-test")]
        public async Task<IActionResult> Handle()
        {
            var metadata = new Dictionary<string, string>
            {
                {"emailFrom", "eShopOn@dapr.io"},
                {"emailTo", "dapr-smtp-binding@example.net"},
                {"subject", $"Your eShopOnDapr order"}
            };
            try
            {
                await _daprClient.InvokeBindingAsync("smtp", "create", "Testing Dapr SMTP Binding", metadata);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Logger.LogInformation($"Cron start @{DateTime.Now}");

            return Ok();
        }
    }
}
