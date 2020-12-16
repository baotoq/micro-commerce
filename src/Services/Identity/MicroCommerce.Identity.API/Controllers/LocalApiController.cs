using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.Identity.API.Controllers
{
    [Route("localApi")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class LocalApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}
