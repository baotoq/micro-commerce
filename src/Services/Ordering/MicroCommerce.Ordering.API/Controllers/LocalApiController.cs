using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.Ordering.API.Controllers
{
    [Route("localApi")]
    public class LocalApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult Get()
        {
            return Ok(new
            {
                Message = "Hello"
            });
        }
    }
}
