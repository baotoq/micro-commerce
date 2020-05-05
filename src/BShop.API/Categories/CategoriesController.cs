using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Categories.Commands.Create;
using BShop.API.Categories.Commands.Delete;
using BShop.API.Categories.Commands.Put;
using BShop.API.Categories.Models;
using BShop.API.Categories.Queries.GetAll;
using BShop.API.Categories.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BShop.API.Categories
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _logger = NullLogger<CategoriesController>.Instance;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);

            _logger.LogInformation("{@result}", result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            _logger.LogInformation("{@result}", result);

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Post(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, PutCategoryCommand request, CancellationToken cancellationToken)
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
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}