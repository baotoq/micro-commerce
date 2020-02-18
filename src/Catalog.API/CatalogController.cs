using Catalog.API.AppServices;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API
{
    [Route("api/catalogs")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private BasketClientService _basketClientService;

        public CatalogController(BasketClientService basketClientService)
        {
            _basketClientService = basketClientService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
