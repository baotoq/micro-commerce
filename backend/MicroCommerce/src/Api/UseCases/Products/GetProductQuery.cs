using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.UseCases.Products;

public record GetProductQuery(string Id) : IRequest<GetProductResponse>
{
    public class Handler(ApplicationDbContext context) : IRequestHandler<GetProductQuery, GetProductResponse>
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
}

public record GetProductResponse(string Id, string Name);