using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.Ordering.API.Controllers
{
    [Authorize]
    [Route("localApi")]
    public class LocalApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}
