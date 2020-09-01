using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Services;
using Data.UnitOfWork;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;

namespace Catalog.API.Application.Carts.Commands
{
    public class UpdateCartQuantityCommand : IRequest<Unit>
    {
        public long CartItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartQuantityCommandValidator : AbstractValidator<UpdateCartQuantityCommand>
    {
        public UpdateCartQuantityCommandValidator()
        {
            RuleFor(s => s.Quantity).GreaterThan(0);
        }
    }

    public class UpdateCartQuantityCommandHandler : IRequestHandler<UpdateCartQuantityCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;

        public UpdateCartQuantityCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IRepository<Cart> cartRepository, IRepository<CartItem> cartItemRepository)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
        }

        public async Task<Unit> Handle(UpdateCartQuantityCommand request, CancellationToken cancellationToken)
        {
            var customerId = _identityService.GetCurrentUserId();

            var cart = await _cartRepository.Query()
                .FindActiveCart(customerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                throw new NotFoundException(nameof(Cart));
            }

            var cartItem = await _cartItemRepository.Query()
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == request.CartItemId, cancellationToken);

            if (request.Quantity > cartItem.Product.CartMaxQuantity)
            {
                throw new Exception($"Can only add {cartItem.Product.CartMaxQuantity} items per cart");
            }

            if (request.Quantity > cartItem.Quantity && request.Quantity > cartItem.Product.SellQuantity)
            {
                throw new Exception($"There are only {cartItem.Product.StockQuantity} items available for {cartItem.Product.Name}");
            }

            cartItem.Quantity = request.Quantity;

            await _unitOfWork.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
