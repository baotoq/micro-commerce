using Application;
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
}