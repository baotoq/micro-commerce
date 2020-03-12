using System.Threading.Tasks;
using Catalog.API.AppServices;
using Catalog.API.Infrastructure;
using Catalog.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Controllers
{
    [Route("api/catalogs")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly BasketClientService _basketClientService;
        private readonly CatalogContext _catalogContext;

        public CatalogController(BasketClientService basketClientService, CatalogContext catalogContext)
        {
            _basketClientService = basketClientService;
            _catalogContext = catalogContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _basketClientService.SayHello();
            await _catalogContext.CatalogItems.AddAsync(new CatalogItem());
            await _catalogContext.SaveChangesAsync();
            return Ok(await _catalogContext.CatalogItems.ToListAsync());
        }
    }
}
