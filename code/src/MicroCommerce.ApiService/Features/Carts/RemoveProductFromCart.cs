using Ardalis.GuardClauses;
using Elastic.Clients.Elasticsearch;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedLockNet.SERedis;

namespace MicroCommerce.ApiService.Features.Carts;

public class RemoveProductToCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/api/carts/{id:guid}/products", async (Guid id, [FromBody] Command request, IMediator mediator)
                =>
            {
                request.CartId = id;
                var result = await mediator.Send(request);
                return TypedResults.Ok(result);
            })
            .ProducesValidationProblem();
    }

    public record Command : IRequest<Response>
    {
        public required Guid ProductId { get; set; }
        public required Guid CartId { get; set; }
        public required long Quantity { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CartId).NotEmpty();
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public record Response
    {
        public Guid CartId { get; init; }
    }

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly RedLockFactory _redLockFactory;

        public Handler(ApplicationDbContext context, ILogger<Handler> logger, RedLockFactory redLockFactory)
        {
            _context = context;
            _logger = logger;
            _redLockFactory = redLockFactory;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var redLock = await _redLockFactory.CreateLockAsync(LockKeys.Cart(request.CartId), TimeSpan.FromMinutes(1));

            if (!redLock.IsAcquired)
            {
                throw new InvalidValidationException("Cart is being processed by another process. Please try again later.");
            }

            await using var trans = await _context.Database.BeginTransactionAsync(cancellationToken);

            var cart = await _context.Carts
                .FirstOrDefaultAsync(s => s.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                throw new NotFoundException(request.CartId.ToString(), "Cart not found");
            }

            if (cart.Status != CartStatus.Pending)
            {
                throw new InvalidValidationException("Cart is not pending");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(s => s.CartId == request.CartId && s.ProductId == request.ProductId, cancellationToken);

            if (cartItem is null)
            {
                return new Response
                {
                    CartId = cart.Id,
                };
            }

            var rowAffected = await _context.CartItems
                .Where(s => s.CartId == request.CartId && s.ProductId == request.ProductId)
                .Where(s => s.ProductQuantity - request.Quantity >= 0)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(p => p.ProductQuantity, p => p.ProductQuantity - request.Quantity), cancellationToken);

            if (rowAffected == 0)
            {
                throw new InvalidValidationException("Update cart item failed!");
            }

            rowAffected = await _context.CartItems
                .Where(s => s.CartId == request.CartId && s.ProductId == request.ProductId)
                .Where(s => s.ProductQuantity - request.Quantity <= 0)
                .ExecuteDeleteAsync(cancellationToken);

            if (rowAffected == 0)
            {
                throw new InvalidValidationException("Update cart item failed!");
            }

            await _context.SaveChangesAsync(cancellationToken);
            await trans.CommitAsync(cancellationToken);

            return new Response
            {
                CartId = cart.Id,
            };
        }
    }
}
