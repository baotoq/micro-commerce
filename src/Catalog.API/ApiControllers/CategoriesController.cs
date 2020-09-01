using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Commands;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Application.Categories.Queries;
using Data.UnitOfWork.EF.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [ResponseCache(Duration = 5)]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<OffsetPaged<CategoryDto>>> FindCategories([FromQuery] FindCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result;
        }

        [ResponseCache(Duration = 5)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> FindCategoryById(long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new FindCategoryByIdQuery(id), cancellationToken);

            return result;
        }

        [ResponseCache(Duration = 5)]
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
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return CreatedAtAction(nameof(FindCategoryById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(long id, UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            request.Id = id;
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(long id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);

            return Ok();
        }
    }
}
