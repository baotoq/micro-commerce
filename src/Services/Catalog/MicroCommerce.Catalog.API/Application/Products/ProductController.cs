using System.Net.Http;
using System.Threading.Tasks;
using Dapr.Client;
using Grpc.Health.V1;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Commands;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Application.Products.Queries;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Services;
using MicroCommerce.Ordering.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Application.Products
{
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class ProductController : BaseController
    {
        public ProductController(ILogger<ProductController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ProductDto> Get(int id)
        {
            return await Mediator.Send(new FindProductByIdQuery
            {
                Id = id
            });
        }

        [AllowAnonymous]
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
        public async Task<ProductDto> Update(UpdateProductCommand request)
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

        [AllowAnonymous]
        [HttpGet("/health/ordering")]
        public async Task<IActionResult> HealthOrdering([FromServices] Health.HealthClient healthClient, [FromServices] IOrderingServiceClient orderingServiceClient)
        {
            await Mediator.Send(new FindProductsQuery());
            var a = await orderingServiceClient.SayHello(new HelloRequest());
            //var result = await healthClient.CheckAsync(new HealthCheckRequest());
            
            return Ok(new {a});
        }
    }
}
