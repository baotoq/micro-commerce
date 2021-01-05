using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Infrastructure
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly ILogger<ControllerBase> Logger;
        protected readonly IMediator Mediator;

        public BaseController(ILogger<ControllerBase> logger, IMediator mediator)
        {
            Logger = logger;
            Mediator = mediator;
            Logger.LogInformation("BaseController constructor");
        }
    }
}
