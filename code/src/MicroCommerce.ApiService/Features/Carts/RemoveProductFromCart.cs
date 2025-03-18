using Ardalis.GuardClauses;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Exceptions;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedLockNet;

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

    public class Handler(ApplicationDbContext context, ILogger<RemoveProductToCart.Handler> logger, IDistributedLockFactory distributedLockFactory) : IRequestHandler<Command, Response>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IDistributedLockFactory _distributedLockFactory = distributedLockFactory;
        private readonly ILogger<Handler> _logger = logger;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var distributedLock = await _distributedLockFactory.CreateLockAsync(LockKeys.Cart(request.CartId), TimeSpan.FromMinutes(1));

            if (!distributedLock.IsAcquired)
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

            var ok = await _context.CartItems.DecreaseProductQuantityInCartAsync(cartItem, request.Quantity, cancellationToken);

            if (!ok)
            {
                throw new InvalidValidationException("Update cart item failed!");
            }

            var rowAffected = await _context.CartItems
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
