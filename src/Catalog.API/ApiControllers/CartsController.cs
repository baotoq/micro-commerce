using Catalog.API.Application.Carts.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("add-to-cart")]
        public async Task<ActionResult> AddToCart(AddToCartCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }

        [HttpPost("remove-from-cart")]
        public async Task<ActionResult> RemoveFromCart(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartQuantityCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return Ok();
        }
    }
}
