using Elastic.Clients.Elasticsearch;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.DomainEvents;
using MicroCommerce.ApiService.Infrastructure;

namespace MicroCommerce.ApiService.Features;

public static class CreateProduct
{
    public record Request : IRequest<Response>
    {
        public string Name { get; init; } = "";
        public decimal Price { get; set; }
        public int RemainingStock { get; set; }
    }

    public record Response(Guid Id);

    public class Handler(ApplicationDbContext context, ILogger<Handler> logger) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                RemainingStock = request.RemainingStock,
                TotalStock = request.RemainingStock,
            };

            var entity = await context.Products.AddAsync(product, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Create product successfully");

            return new Response(entity.Entity.Id);
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.RemainingStock).GreaterThan(0);
        }
    }
}
