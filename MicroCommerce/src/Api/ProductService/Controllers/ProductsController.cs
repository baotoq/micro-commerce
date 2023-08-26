using Application;
using Application.Common;
using Application.UseCases.Products;
using Application.UseCases.Products.Commands;
using Application.UseCases.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Controllers;


public class ProductsController : ApiController
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpGet]
    public Task<GetProductsQuery.Response> GetAll(CancellationToken cancellationToken)
    {
        return Mediator.Send(new GetProductsQuery(), cancellationToken);
    }

    [HttpPost]
    public Task<CreateProductCommand.Response> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        return Mediator.Send(command, cancellationToken);
    }
}