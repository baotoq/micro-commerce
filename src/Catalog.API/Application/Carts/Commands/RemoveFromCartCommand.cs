using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Services;
using Data.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;

namespace Catalog.API.Application.Carts.Commands
{
    public class RemoveFromCartCommand : IRequest<Unit>
    {
        public long CartItemId { get; set; }
    }

    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;

        public RemoveFromCartCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork,IRepository<Cart> cartRepository, IRepository<CartItem> cartItemRepository)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
        }

        public async Task<Unit> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var customerId = _identityService.GetCurrentUserId();

            var cart = await _cartRepository.Query()
                .FindActiveCart(customerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                throw new Exception(nameof(Cart));
            }

            if (cart.LockedOnCheckout)
            {
                throw new Exception("Cart is being locked for checkout. Please complete the checkout first");
            }

            var cartItem = await _cartItemRepository.FindAsync(request.CartItemId, cancellationToken);

            if (cartItem == null)
            {
                throw new NotFoundException(nameof(CartItem), request.CartItemId);
            }

            _cartItemRepository.Remove(cartItem);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
