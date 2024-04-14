using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Products;

public record GetProductQuery(string Id) : IRequest<GetProductResponse>
{
    public static Func<IMediator, string, Task<GetProductResponse>> EndpointHandler => (mediator, id) => mediator.Send(new GetProductQuery(id));
}

public class GetProductQueryHandler(ApplicationDbContext context) : IRequestHandler<GetProductQuery, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new Exception("Not found");
        }
            
        return new GetProductResponse(product.Id, product.Name);
    }
}

public record GetProductResponse(string Id, string Name);