using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.UseCases.Carts.DomainEvents;
using Microsoft.EntityFrameworkCore;
using RedLockNet.SERedis;

namespace MicroCommerce.ApiService.UseCases.Carts;

public record CheckoutACartCommand : IRequest<CheckoutACartResponse>
{
    public string CartId { get; set; } = "";
    public string DeliveryOptionId { get; set; } = "";
    public DeliveryAddressViewModel DeliveryAddress { get; set; } = new();
    
    public class DeliveryAddressViewModel
    {
        public string AddressLine { get; set; } = "";
        public string City { get; set; } = "";
        public string RecipientName { get; set; } = "";
        public string RecipientPhoneNumber { get; set; } = "";
        public string DeliveryInstruction { get; set; } = "";
    }
    
    public class Handler(ApplicationDbContext context, RedLockFactory redLockFactory) : IRequestHandler<CheckoutACartCommand, CheckoutACartResponse>
    {
        public async Task<CheckoutACartResponse> Handle(CheckoutACartCommand request, CancellationToken cancellationToken)
        {
            await using var redLock = await redLockFactory.CreateLockAsync(LockKey.Cart(request.CartId), TimeSpan.FromSeconds(30));

            if (!redLock.IsAcquired)
            {
                throw new Exception("redlock is not acquired");
            }
            
            var deliveryOption = await context.DeliveryOptions
                .Where(s => s.Id == request.DeliveryOptionId)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (deliveryOption == null)
            {
                throw new Exception("DeliveryOption not found");
            }
            
            await using var trans = await context.Database.BeginTransactionAsync(cancellationToken);
         
            var cart = await context.Carts
                .Include(s => s.CartItems)
                .ThenInclude(s => s.Product)
                .Where(s => s.Id == request.CartId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            cart.DeliveryAddress = new DeliveryAddress
            {
                AddressLine = request.DeliveryAddress.AddressLine,
                City = request.DeliveryAddress.City,
                RecipientName = request.DeliveryAddress.RecipientName,
                RecipientPhoneNumber = request.DeliveryAddress.RecipientPhoneNumber,
                DeliveryInstruction = request.DeliveryAddress.DeliveryInstruction
            };

            foreach (var cartItem in cart.CartItems)
            {
                cartItem.ProductPriceAtCheckoutTime = cartItem.Product.Price;
                
                cartItem.Product.UseRemainingStock(cartItem.ProductQuantity);
            }

            cart.Status = CartStatus.Paid;

            cart.DeliveryOptionId = deliveryOption.Id;
            cart.DeliveryFee = deliveryOption.Fee;
            cart.SubTotal = cart.CartItems.Sum(s => s.ProductQuantity * s.Product.Price);
            
            cart.TotalCheckoutAmount = Math.Max(0, Math.Round(cart.SubTotal - cart.TotalPromotionDiscountAmount, 2) + cart.DeliveryFee);
            
            cart.AddDomainEvent(new CartCheckedOutDomainEvent
            {
                CartId = cart.Id
            });
            
            await context.SaveChangesAsync(cancellationToken);
            await trans.CommitAsync(cancellationToken);
            
            return new CheckoutACartResponse(cart.Id);
        }
    }
}

public class CheckoutACartCommandValidator : AbstractValidator<CheckoutACartCommand>
{
    public CheckoutACartCommandValidator()
    {
        RuleFor(x => x.CartId).NotEmpty();
        RuleFor(x => x.DeliveryOptionId).NotEmpty();
        RuleFor(x => x.DeliveryAddress.AddressLine).NotEmpty();
        RuleFor(x => x.DeliveryAddress.City).NotEmpty();
        RuleFor(x => x.DeliveryAddress.RecipientName).NotEmpty();
        RuleFor(x => x.DeliveryAddress.RecipientPhoneNumber).NotEmpty();
        RuleFor(x => x.DeliveryAddress.DeliveryInstruction).MaximumLength(200);
    }
}

public record CheckoutACartResponse(string CartId);