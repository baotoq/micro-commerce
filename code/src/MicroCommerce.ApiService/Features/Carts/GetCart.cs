using Ardalis.GuardClauses;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Carts;

public class GetCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/carts/{id:guid}", async (Guid id, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new Query { Id = id })));
    }

    public record Query : IRequest<Response>
    {
        public required Guid Id { get; init; }
    }

    public record Response
    {
        public Guid Id { get; init; }
        public CartStatus Status { get; init; }
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
            var cart = await _context.Carts
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (cart is null)
            {
                throw new NotFoundException(request.Id.ToString(), "Cart not found");
            }

            return new Response
            {
                Id = cart.Id,
                Status = cart.Status
            };
        }
    }
}
