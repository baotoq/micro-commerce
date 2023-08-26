using Application;
using Application.UseCases.Carts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

public class CartsController : ApiController
{
    public CartsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<GetCartsQuery.Response> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCartsQuery(), cancellationToken);
        return result;
    }
}