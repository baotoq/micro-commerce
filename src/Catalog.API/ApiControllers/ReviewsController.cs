using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Reviews.Commands;
using Catalog.API.Application.Reviews.Models;
using Catalog.API.Application.Reviews.Queries;
using Data.UnitOfWork.EF.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [ResponseCache(Duration = 5)]
        [AllowAnonymous]
        [HttpGet("cursor")]
        public async Task<ActionResult<CursorPaged<ReviewDto, DateTime?>>> FindReviews([FromQuery] FindReviewsCursorQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result;
        }

        [ResponseCache(Duration = 5)]
        [AllowAnonymous]
        [HttpGet("offset")]
        public async Task<ActionResult<OffsetPaged<ReviewDto>>> FindReviews([FromQuery] FindReviewsOffsetQuery request, CancellationToken cancellationToken)
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(long id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteReviewCommand(id), cancellationToken);

            return Ok();
        }

        [HttpPost("{id}/change-review-status")]
        public async Task<ActionResult> ChangeReviewStatus(long id, ChangeReviewStatusCommand request, CancellationToken cancellationToken)
        {
            request.Id = id;
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }
    }
}
