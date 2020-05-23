using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Products.Commands.Create;
using BShop.API.Application.Products.Commands.Delete;
using BShop.API.Application.Products.Commands.Put;
using BShop.API.Application.Products.Models;
using BShop.API.Application.Products.Queries.GetAll;
using BShop.API.Application.Products.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BShop.API.Application.Products
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _logger = NullLogger<ProductsController>.Instance;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);

            _logger.LogInformation("{@result}", result);

            return result;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> Get(long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            _logger.LogInformation("{@result}", result);

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Post(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, PutProductCommand request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _mediator.Send(request, cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
