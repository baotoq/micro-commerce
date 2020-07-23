using Catalog.API.Application.Orders.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Orders.Models;
using Catalog.API.Application.Orders.Queries;
using UnitOfWork.Common;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<OffsetPaged<OrderDto>>> FindOrders([FromQuery] FindOrdersQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            return result;
        }

        [HttpPost("create-order")]
        public async Task<ActionResult> CreateOrder(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);
            return Ok();
        }

        [HttpPut("{id}/change-status")]
        public async Task<ActionResult> ChangeOrderStatus(long id, ChangeOrderStatusCommand request, CancellationToken cancellationToken)
        {
            request.Id = id;
            await _mediator.Send(request, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(long id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteOrderCommand(id), cancellationToken);
            return Ok();
        }
    }
}
