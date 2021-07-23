using IdentityServer4;
using MicroCommerce.Identity.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.Identity.API.Controllers
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [Route("api/localApi")]
    public class LocalApiController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public LocalApiController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello");
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Hello");
        }
    }
}
