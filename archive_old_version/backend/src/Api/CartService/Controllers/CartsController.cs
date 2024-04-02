using Application;
using Application.Common;
using Application.UseCases.Carts.Commands;
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
    public Task<GetCartsQuery.Response> GetAll(CancellationToken cancellationToken)
    {
        return Mediator.Send(new GetCartsQuery(), cancellationToken);
    }

    [HttpPost]
    public Task AddProductToCart(AddProductToCartCommand request, CancellationToken cancellationToken)
    {
        return Mediator.Send(request, cancellationToken);
    }
}