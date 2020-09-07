using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.IntegrationEvents.Models;
using Catalog.API.Data.Models.Enums;
using Data.UnitOfWork.EF.Core;
using MassTransit;

namespace Catalog.API.Application.Orders.Commands
{
    public class CreateOrderCommand : IRequest<Unit>
    {
        public long CartId { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IBus _bus;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IRepository<Cart> cartRepository, IRepository<Order> orderRepository, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _bus = bus;
        }

        public async Task<Unit> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.Query()
                .Include(s => s.Items)
                .ThenInclude(s => s.Product)
                .SingleOrDefaultAsync(s => s.Id == request.CartId, cancellationToken);

            if (cart == null)
            {
                throw new NotFoundException(nameof(Cart), request.CartId);
            }

            cart.IsActive = false;

            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.Items)
            {
                var product = cartItem.Product;

                if (cartItem.Quantity > product.CartMaxQuantity)
                {
                    throw new Exception($"Can only add {product.CartMaxQuantity} items per cart");
                }

                if (cartItem.Quantity > cartItem.Product.StockQuantity)
                {
                    throw new Exception($"There are only {product.StockQuantity} items available for {product.Name}");
                }

                product.StockQuantity -= cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductPrice = product.Price,
                    Quantity = cartItem.Quantity
                });
            }

            var order = new Order
            {
                CustomerId = cart.CustomerId,
                OrderNote = cart.OrderNote,
                OrderItems = orderItems,
                SubTotal = orderItems.Sum(s => s.ProductPrice * s.Quantity),
                OrderStatus = OrderStatus.New
            };

            await _orderRepository.AddAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _bus.Publish(new OrderCreated
            {
                OrderId = order.Id
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
