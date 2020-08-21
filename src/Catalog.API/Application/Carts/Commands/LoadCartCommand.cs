using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Carts.Models;
using Catalog.API.Data.Models;
using Catalog.API.Services;
using Data.UnitOfWork.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Application.Carts.Commands
{
    public class LoadCartCommand : IRequest<CartDto>
    {
    }

    public class LoadCartCommandHandler : IRequestHandler<LoadCartCommand, CartDto>
    {
        private readonly IIdentityService _identityService;
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<Cart> _cartRepository;

        public LoadCartCommandHandler(IIdentityService identityService, IEfUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _cartRepository = unitOfWork.Repository<Cart>();
        }

        public async Task<CartDto> Handle(LoadCartCommand request, CancellationToken cancellationToken)
        {
            var customerId = _identityService.GetCurrentUserId();

            var cart = await _cartRepository.Query()
                .FindActiveCart(customerId)
                .Select(s => new CartDto
                {
                    Id = s.Id,
                    Items = s.Items.Select(x => new CartItemDto
                    {
                        Id = x.Id,
                        Quantity = x.Quantity,
                        Product = new ProductDto
                        {
                            Name = x.Product.Name,
                            Price = x.Product.Price,
                            CartMaxQuantity = x.Product.CartMaxQuantity,
                            ImageUri = x.Product.ImageUri,
                            Description = x.Product.Description
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                var newCart = new Cart
                {
                    CustomerId = customerId
                };
                await _cartRepository.AddAsync(newCart, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new CartDto
                {
                    Id = newCart.Id,
                };
            }

            return cart;
        }
    }
}
