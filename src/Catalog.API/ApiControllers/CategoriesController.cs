using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Commands.Create;
using Catalog.API.Application.Categories.Commands.Delete;
using Catalog.API.Application.Categories.Commands.Put;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork.Common;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<CursorPaged<CategoryDto, long?>>> Find([FromQuery] FindCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> FindById(long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new FindCategoryByIdQuery(id), cancellationToken);

            return result;
        }

        [AllowAnonymous]
        [HttpGet("{id}/products")]
        public async Task<ActionResult<OffsetPaged<ProductDto>>> FindProducts(long id, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new FindProductsByCategoryIdQuery
            {
                Id = id,
                Page = page,
                PageSize = pageSize
            }, cancellationToken);

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Post(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return CreatedAtAction(nameof(FindById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, PutCategoryCommand request, CancellationToken cancellationToken)
        {
            request.Id = id;
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);

            return NoContent();
        }
    }
}
