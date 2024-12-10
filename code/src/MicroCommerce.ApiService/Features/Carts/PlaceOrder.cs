using Ardalis.GuardClauses;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Features.DomainEvents;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedLockNet;

namespace MicroCommerce.ApiService.Features.Carts;

public class PlaceOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/carts/{id:guid}/place-order", async (Guid id, [FromBody] Command request, IMediator mediator)
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
        public required Guid CartId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CartId).NotEmpty();
        }
    }

    public record Response
    {
        public Guid CartId { get; init; }
    }

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedLockFactory _distributedLockFactory;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDbContext context, ILogger<Handler> logger, IDistributedLockFactory distributedLockFactory)
        {
            _context = context;
            _logger = logger;
            _distributedLockFactory = distributedLockFactory;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var distributedLock = await _distributedLockFactory.CreateLockAsync(LockKeys.Cart(request.CartId), TimeSpan.FromMinutes(1));

            if (!distributedLock.IsAcquired)
            {
                throw new InvalidValidationException("Cart is being processed by another process. Please try again later.");
            }

            await using var trans = await _context.Database.BeginTransactionAsync(cancellationToken);

            var cart = await _context.Carts
                .Include(cart => cart.CartItems)
                .ThenInclude(cartItem => cartItem.Product)
                .FirstOrDefaultAsync(s => s.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                throw new NotFoundException(request.CartId.ToString(), "Cart not found");
            }

            if (cart.Status != CartStatus.Pending)
            {
                throw new InvalidValidationException("Cart is not pending");
            }

            cart.Status = CartStatus.Paid;
            cart.CheckoutAt = DateTimeOffset.UtcNow;

            foreach (var cartItem in cart.CartItems)
            {
                cartItem.ProductPriceAtCheckoutTime = cartItem.Product.Price;
                var ok = await _context.Products.UseProductRemainingStockAsync(cartItem.ProductId, cartItem.ProductQuantity, cancellationToken);
                if (!ok)
                {
                    throw new InvalidValidationException($"Stock of {cartItem.ProductId} is not available");
                }
            }

            cart.AddDomainEvent(new OrderCreatedDomainEvent(cart.Id));

            await _context.SaveChangesAsync(cancellationToken);
            await trans.CommitAsync(cancellationToken);

            return new Response
            {
                CartId = cart.Id,
            };
        }
    }
}
