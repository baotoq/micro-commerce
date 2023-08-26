using Application;
using Application.UseCases.Products;
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
    public async Task<GetProductsQueryResponse> GetProducts(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetProductsQuery(), cancellationToken);
        return result;
    }
}