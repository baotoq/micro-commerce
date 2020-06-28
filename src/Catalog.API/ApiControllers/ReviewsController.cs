using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Reviews.Commands;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.Application.Reviews.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork.Common;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<CursorPaged<ReviewDto, DateTime?>>> FindReviews([FromQuery] FindReviewsQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result;
        }

        [HttpPost]
        public async Task<ActionResult> CreateReview(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }

        [HttpPost("change-review-status")]
        public async Task<ActionResult> ChangeReviewStatus(ChangeReviewStatusCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }
    }
}
