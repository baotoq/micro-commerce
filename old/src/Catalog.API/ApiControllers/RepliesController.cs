using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Replies.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RepliesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RepliesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> CreateReply(CreateReplyCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }
    }
}
