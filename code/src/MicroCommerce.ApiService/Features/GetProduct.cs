using Ardalis.GuardClauses;
using MassTransit;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;

namespace MicroCommerce.ApiService.Features;

public static class GetProduct
{
    public class Request : IRequest<Response>
    {
        public Guid Id { get; set; }
    }

    public record Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
    }
    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
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
