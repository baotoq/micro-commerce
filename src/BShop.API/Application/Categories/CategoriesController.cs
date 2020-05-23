using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Categories.Commands.Create;
using BShop.API.Application.Categories.Commands.Delete;
using BShop.API.Application.Categories.Commands.Put;
using BShop.API.Application.Categories.Models;
using BShop.API.Application.Categories.Queries.GetAll;
using BShop.API.Application.Categories.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BShop.API.Application.Categories
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);

            _logger.LogInformation("{@result}", result);

            return result;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> Get(long id, CancellationToken cancellationToken)
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
        public async Task<IActionResult> Put(long id, PutCategoryCommand request, CancellationToken cancellationToken)
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
            var result = await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
