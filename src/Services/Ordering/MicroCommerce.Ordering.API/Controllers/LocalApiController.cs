using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.Ordering.API.Controllers
{
    [Route("localApi")]
    public class LocalApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new HelloReply
            {
                Message = "Hello"
            });
        }

        [HttpGet("2")]
        public IActionResult Get2()
        {
            return Ok(new HelloReply
            {
                Message = "Hello 2"
            });
        }
    }
}
