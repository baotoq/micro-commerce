using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Infrastructure
{
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        protected readonly ILogger<ControllerBase> Logger;
        protected readonly IMediator Mediator;

        public ApiControllerBase(ILogger<ControllerBase> logger, IMediator mediator)
        {
            Logger = logger;
            Mediator = mediator;
        }
    }
}
