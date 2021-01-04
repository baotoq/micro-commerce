using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Health.V1;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    public class ProductController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly Health.HealthClient _healthClient;

        public ProductController(ILogger<ProductController> logger, Health.HealthClient healthClient, ApplicationDbContext context) : base(logger)
        {
            _healthClient = healthClient;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<Product> Get(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpPost]
        public async Task<Product> Create(Product payload)
        {
            await _context.Products.AddAsync(payload);
            await _context.SaveChangesAsync();
            return payload;
        }

        [HttpPut]
        public async Task<Product> Update(Product payload)
        {
            var product = await _context.Products.FindAsync(payload.Id);
            product.Name = payload.Name;
            product.Description = payload.Description;
            product.Price = payload.Price;
            product.StockQuantity = payload.StockQuantity;
            await _context.SaveChangesAsync();
            return payload;
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("/health/ordering")]
        public async Task<IActionResult> HealthOrdering()
        {
            var result = await _healthClient.CheckAsync(new HealthCheckRequest());
            return Ok(result);
        }
    }
}
