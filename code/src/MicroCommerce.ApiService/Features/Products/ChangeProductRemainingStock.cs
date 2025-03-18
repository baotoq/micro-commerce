using Ardalis.GuardClauses;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Products;

public class ChangeProductRemainingStock : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/products/stock", async ([FromBody] Command request, IMediator mediator) => TypedResults.Ok(await mediator.Send(request)))
            .ProducesValidationProblem();
    }

    public record Command : IRequest<Response>
    {
        public required Guid ProductId { get; set; }
        public required long ChangeQuantity { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.ChangeQuantity).InclusiveBetween(-500_000, 500_000);
        }
    }

    public record Response
    {
        public Guid ProductId { get; init; }
        public long RemainingStock { get; init; }
    }

    public class Handler(ApplicationDbContext context, ILogger<ChangeProductRemainingStock.Handler> logger) : IRequestHandler<Command, Response>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<Handler> _logger = logger;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(s => s.Id == request.ProductId && !s.DeletedAt.HasValue, cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(request.ProductId.ToString(), "Product not found");
            }

            var rowEffected = await _context.Products.ChangeProductStockAsync(request.ProductId, request.ChangeQuantity, cancellationToken);
            if (rowEffected == 0)
            {
                throw new Exception("Update remaining stock failed!");
            }

            product = await _context.Products
                .FirstOrDefaultAsync(s => s.Id == request.ProductId && !s.DeletedAt.HasValue, cancellationToken);

            _logger.LogInformation("Change product remaining stock successfully");

            return new Response
            {
                ProductId = product!.Id,
                RemainingStock = product.RemainingStock
            };
        }
    }
}
