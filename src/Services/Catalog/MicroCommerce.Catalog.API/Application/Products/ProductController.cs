using System.Threading.Tasks;
using Grpc.Health.V1;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Commands;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Application.Products.Queries;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Application.Products
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly Health.HealthClient _healthClient;

        public ProductController(ILogger<ProductController> logger, IMediator mediator, Health.HealthClient healthClient, ApplicationDbContext context) : base(logger, mediator)
        {
            _healthClient = healthClient;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ProductDto> Get(int id)
        {
            return await Mediator.Send(new FindProductByIdQuery
            {
                Id = id
            });
        }

        [HttpGet]
        public async Task<OffsetPaged<ProductDto>> Get([FromQuery] FindProductsQuery request)
        {
            return await Mediator.Send(request);
        }

        [HttpPost]
        public async Task<ProductDto> Create(CreateProductCommand request)
        {
            return await Mediator.Send(request);
        }

        [HttpPut]
        public async Task<ProductDto> Update(CreateProductCommand request)
        {
            return await Mediator.Send(request);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await Mediator.Send(new DeleteProductCommand
            {
                Id = id
            });
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
