using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RedLockNet.SERedis;

namespace MicroCommerce.ApiService.UseCases.Products;

public record GetProductsQuery : IRequest<IList<GetProductsItemResponse>>
{
    public static Func<IMediator, Task<IList<GetProductsItemResponse>>> EndpointHandler => (mediator) => mediator.Send(new GetProductsQuery());
}

public class GetProductsQueryHandler(ApplicationDbContext context) : IRequestHandler<GetProductsQuery, IList<GetProductsItemResponse>>
{
    public async Task<IList<GetProductsItemResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await context.Products
            .ToListAsync(cancellationToken);

        return products.ConvertAll(s => new GetProductsItemResponse(s.Id, s.Name));
    }
}

public record GetProductsItemResponse(string Id, string Name);