using Application;
using Application.Carts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

public class CartsController : ApiController
{
    public CartsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetCartsQuery(), cancellationToken);
        return Ok(result);
    }
}