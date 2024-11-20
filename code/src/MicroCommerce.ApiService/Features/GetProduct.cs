using Ardalis.GuardClauses;
using MassTransit;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Features;

public class GetProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/products/{id:guid}", async (Guid id, IMediator mediator) => await mediator.Send(new Query { Id = id }));
    }

    public class Query : IRequest<Response>
    {
        public required Guid Id { get; set; }
    }

    public record Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
    }

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(request.Id);

            if (product is null)
            {
                throw new NotFoundException(request.Id.ToString(), "Product not found");
            }

            return new Response
            {
                Id = product.Id,
                Name = product.Name
            };
        }
    }
}
