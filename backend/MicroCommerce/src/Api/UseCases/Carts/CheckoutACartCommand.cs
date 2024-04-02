using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RedLockNet.SERedis;

namespace Api.UseCases.Carts;

public record CheckoutACartCommand : IRequest<CheckoutACartResponse>
{
    public string CartId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int ProductQuantity { get; set; }
    
    public class Handler(ApplicationDbContext context, RedLockFactory redLockFactory) : IRequestHandler<CheckoutACartCommand, CheckoutACartResponse>
    {
        public async Task<CheckoutACartResponse> Handle(CheckoutACartCommand request, CancellationToken cancellationToken)
        {
            await using var redLock = await redLockFactory.CreateLockAsync(LockKey.Cart(request.CartId), TimeSpan.FromSeconds(30));

            if (!redLock.IsAcquired)
            {
                throw new Exception("redlock is not acquired");
            }
            
            await using var trans = await context.Database.BeginTransactionAsync(cancellationToken);
         
            var cart = await context.Carts
                .Where(s => s.Id == request.CartId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            cart.Status = CartStatus.Paid;
            
            await context.SaveChangesAsync(cancellationToken);
            await trans.CommitAsync(cancellationToken);
            
            return new CheckoutACartResponse(cart.Id);
        }
    }
}

public record CheckoutACartResponse(string CartId);