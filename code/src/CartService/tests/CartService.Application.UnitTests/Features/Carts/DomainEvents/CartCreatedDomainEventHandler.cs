using MicroCommerce.BuildingBlocks.Common.Events;
using MicroCommerce.CartService.Application.Features.Carts.DomainEvents;
using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Carts.DomainEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CartService.Application.UnitTests.Features.Carts.DomainEvents;

public class CartCreatedDomainEventHandlerTests
{
    private readonly CartCreatedDomainEventHandler _sut;

    public CartCreatedDomainEventHandlerTests()
    {
        _sut = new CartCreatedDomainEventHandler(NullLogger<CartCreatedDomainEventHandler>.Instance);
    }

    [Fact]
    public async Task Handle()
    {
        // Arrange
        var cartCreatedEvent = new CartCreatedDomainEvent(CartId.New());

        // Act
        await _sut.Handle(cartCreatedEvent, CancellationToken.None);

        // Assert
    }
}
