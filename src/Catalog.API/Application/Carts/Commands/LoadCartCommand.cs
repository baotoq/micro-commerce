using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Carts.Models;
using Catalog.API.Data.Models;
using Catalog.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Catalog.API.Application.Carts.Commands
{
    public class LoadCartCommand : IRequest<List<CartItemDto>>
    {
    }

    public class LoadCartCommandHandler : IRequestHandler<LoadCartCommand, List<CartItemDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IRepository<Cart> _cartRepository;

        public LoadCartCommandHandler(IIdentityService identityService, IRepository<Cart> cartRepository)
        {
            _identityService = identityService;
            _cartRepository = cartRepository;
        }

        public async Task<List<CartItemDto>> Handle(LoadCartCommand request, CancellationToken cancellationToken)
        {
            var customerId = _identityService.GetCurrentUserId();

            var cart = await _cartRepository.Query()
                .FindActiveCart(customerId)
                .Select(s => new CartDto
                {
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

            return cart.Items.ToList();
        }
    }
}
