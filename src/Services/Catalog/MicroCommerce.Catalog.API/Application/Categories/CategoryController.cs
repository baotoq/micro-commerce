using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Catalog.API.Application.Categories.Models;
using MicroCommerce.Catalog.API.Application.Categories.Queries;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.API.Application.Categories
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    [TranslateResultToActionResult]
    public class CategoryController : BaseController
    {
        public CategoryController(ILogger<CategoryController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<Result<IEnumerable<CategoryDto>>> Get()
        {
            return await Mediator.Send(new FindCategoriesQuery());
        }
    }
}
