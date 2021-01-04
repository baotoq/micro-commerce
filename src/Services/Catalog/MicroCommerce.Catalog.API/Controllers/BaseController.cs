using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly ILogger<ControllerBase> Logger;

        public BaseController(ILogger<ControllerBase> logger)
        {
            Logger = logger;
            Logger.LogInformation("BaseController constructor");
        }
    }
}
