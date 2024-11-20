using Elastic.Clients.Elasticsearch;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.DomainEvents;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.ApiService.Features;

public class CreateProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/products", async ([FromBody] Command request, IMediator mediator) =>
        {
            var response = await mediator.Send(request);
            return Results.CreatedAtRoute();
        }).Produces(StatusCodes.Status201Created);
    }

    public record Command : IRequest<Response>
    {
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required int RemainingStock { get; set; }
    }

    public record Response(Guid Id);

    public class Handler(ApplicationDbContext context, ILogger<Handler> logger) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
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

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.RemainingStock).GreaterThan(0);
        }
    }
}
