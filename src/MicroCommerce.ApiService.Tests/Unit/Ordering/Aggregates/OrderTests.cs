using FluentAssertions;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Domain.Events;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Ordering.Aggregates;

[Trait("Category", "Unit")]
public sealed class OrderTests
{
    [Fact]
    public void Create_ValidData_ReturnsOrderWithSubmittedStatus()
    {
        // Arrange
        Order order = CreateValidOrder();

        // Assert
        order.Status.Should().Be(OrderStatus.Submitted);
        order.OrderNumber.Should().NotBeNull();
        order.BuyerId.Should().NotBeEmpty();
        order.BuyerEmail.Should().Be("buyer@test.com");
        order.ShippingAddress.Should().NotBeNull();
        order.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ValidData_RaisesOrderSubmittedDomainEvent()
    {
        // Arrange & Act
        Order order = CreateValidOrder();

        // Assert
        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents.Should().ContainSingle(e => e is OrderSubmittedDomainEvent);
    }

    [Fact]
    public void Create_ValidData_CalculatesSubtotalCorrectly()
    {
        // Arrange
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
        [
            (Guid.NewGuid(), "Product A", 10.00m, null, 2),
            (Guid.NewGuid(), "Product B", 25.50m, null, 3),
            (Guid.NewGuid(), "Product C", 7.99m, null, 1)
        ];

        decimal expectedSubtotal = (10.00m * 2) + (25.50m * 3) + (7.99m * 1); // 104.49

        // Act
        Order order = Order.Create(
            Guid.NewGuid(),
            "buyer@test.com",
            CreateValidAddress(),
            items);

        // Assert
        order.Subtotal.Should().Be(expectedSubtotal);
    }

    [Fact]
    public void Create_ValidData_CalculatesTaxAt8Percent()
    {
        // Arrange
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
        [
            (Guid.NewGuid(), "Product A", 100.00m, null, 1)
        ];

        decimal expectedTax = Math.Round(100.00m * 0.08m, 2); // 8.00

        // Act
        Order order = Order.Create(
            Guid.NewGuid(),
            "buyer@test.com",
            CreateValidAddress(),
            items);

        // Assert
        order.Tax.Should().Be(expectedTax);
    }

    [Fact]
    public void Create_ValidData_CalculatesShippingAt599()
    {
        // Arrange & Act
        Order order = CreateValidOrder();

        // Assert
        order.ShippingCost.Should().Be(5.99m);
    }

    [Fact]
    public void Create_ValidData_CalculatesTotalCorrectly()
    {
        // Arrange
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
        [
            (Guid.NewGuid(), "Product A", 50.00m, null, 2)
        ];

        decimal subtotal = 100.00m;
        decimal shipping = 5.99m;
        decimal tax = Math.Round(100.00m * 0.08m, 2); // 8.00
        decimal expectedTotal = subtotal + shipping + tax; // 113.99

        // Act
        Order order = Order.Create(
            Guid.NewGuid(),
            "buyer@test.com",
            CreateValidAddress(),
            items);

        // Assert
        order.Total.Should().Be(expectedTotal);
        order.Total.Should().Be(order.Subtotal + order.ShippingCost + order.Tax);
    }

    [Fact]
    public void Create_EmptyItems_ThrowsInvalidOperationException()
    {
        // Arrange
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> emptyItems = [];

        // Act
        Action act = () => Order.Create(
            Guid.NewGuid(),
            "buyer@test.com",
            CreateValidAddress(),
            emptyItems);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order must contain at least one item.");
    }

    [Fact]
    public void Create_NullEmail_ThrowsArgumentException()
    {
        // Arrange
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
        [
            (Guid.NewGuid(), "Product A", 10.00m, null, 1)
        ];

        // Act
        Action act = () => Order.Create(
            Guid.NewGuid(),
            null!,
            CreateValidAddress(),
            items);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NullAddress_ThrowsArgumentNullException()
    {
        // Arrange
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
        [
            (Guid.NewGuid(), "Product A", 10.00m, null, 1)
        ];

        // Act
        Action act = () => Order.Create(
            Guid.NewGuid(),
            "buyer@test.com",
            null!,
            items);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MarkAsPaid_WhenSubmitted_TransitionsToPaid()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenStockReserved_TransitionsToPaid()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.StockReserved);

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenSubmitted_SetsPaidAt()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);

        // Act
        order.MarkAsPaid();

        // Assert
        order.PaidAt.Should().NotBeNull();
        order.PaidAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsPaid_WhenSubmitted_RaisesOrderPaidDomainEvent()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);
        order.ClearDomainEvents();

        // Act
        order.MarkAsPaid();

        // Assert
        order.DomainEvents.Should().ContainSingle(e => e is OrderPaidDomainEvent);
    }

    [Fact]
    public void MarkAsPaid_WhenPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Paid);

        // Act
        Action act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark order as paid when status is 'Paid'*");
    }

    [Fact]
    public void MarkAsPaid_WhenShipped_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Shipped);

        // Act
        Action act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark order as paid when status is 'Shipped'*");
    }

    [Fact]
    public void MarkAsFailed_WhenSubmitted_TransitionsToFailed()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);

        // Act
        order.MarkAsFailed("Stock unavailable");

        // Assert
        order.Status.Should().Be(OrderStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_WhenSubmitted_SetsFailureReason()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);
        string reason = "Payment declined";

        // Act
        order.MarkAsFailed(reason);

        // Assert
        order.FailureReason.Should().Be(reason);
    }

    [Fact]
    public void MarkAsFailed_WhenSubmitted_RaisesOrderFailedDomainEvent()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);
        order.ClearDomainEvents();

        // Act
        order.MarkAsFailed("Stock unavailable");

        // Assert
        order.DomainEvents.Should().ContainSingle(e => e is OrderFailedDomainEvent);
    }

    [Fact]
    public void MarkAsFailed_WhenConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Confirmed);

        // Act
        Action act = () => order.MarkAsFailed("Test reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark order as failed when status is 'Confirmed'*");
    }

    [Fact]
    public void MarkAsFailed_WhenShipped_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Shipped);

        // Act
        Action act = () => order.MarkAsFailed("Test reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark order as failed when status is 'Shipped'*");
    }

    [Fact]
    public void MarkAsFailed_WhenDelivered_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Delivered);

        // Act
        Action act = () => order.MarkAsFailed("Test reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark order as failed when status is 'Delivered'*");
    }

    [Fact]
    public void MarkAsFailed_NullReason_ThrowsArgumentException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);

        // Act
        Action act = () => order.MarkAsFailed(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Confirm_WhenPaid_TransitionsToConfirmed()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Paid);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenSubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);

        // Act
        Action act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot confirm order when status is 'Submitted'*");
    }

    [Fact]
    public void Ship_WhenConfirmed_TransitionsToShipped()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Confirmed);

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_WhenPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Paid);

        // Act
        Action act = () => order.Ship();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot ship order when status is 'Paid'*");
    }

    [Fact]
    public void Deliver_WhenShipped_TransitionsToDelivered()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Shipped);

        // Act
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Deliver_WhenConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Confirmed);

        // Act
        Action act = () => order.Deliver();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot deliver order when status is 'Confirmed'*");
    }

    [Fact]
    public void MarkStockReserved_WhenSubmitted_TransitionsToStockReserved()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Submitted);

        // Act
        order.MarkStockReserved();

        // Assert
        order.Status.Should().Be(OrderStatus.StockReserved);
    }

    [Fact]
    public void MarkStockReserved_WhenPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        Order order = CreateOrderInStatus(OrderStatus.Paid);

        // Act
        Action act = () => order.MarkStockReserved();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark stock reserved when status is 'Paid'*");
    }

    // Helper methods

    private static Order CreateValidOrder()
    {
        List<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
        [
            (Guid.NewGuid(), "Test Product", 50.00m, null, 1)
        ];

        return Order.Create(
            Guid.NewGuid(),
            "buyer@test.com",
            CreateValidAddress(),
            items);
    }

    private static Order CreateOrderInStatus(OrderStatus targetStatus)
    {
        Order order = CreateValidOrder();
        order.ClearDomainEvents();

        // Walk the order through state transitions to reach target status
        switch (targetStatus)
        {
            case OrderStatus.Submitted:
                // Already in Submitted state
                break;

            case OrderStatus.StockReserved:
                order.MarkStockReserved();
                break;

            case OrderStatus.Paid:
                order.MarkAsPaid();
                break;

            case OrderStatus.Confirmed:
                order.MarkAsPaid();
                order.Confirm();
                break;

            case OrderStatus.Shipped:
                order.MarkAsPaid();
                order.Confirm();
                order.Ship();
                break;

            case OrderStatus.Delivered:
                order.MarkAsPaid();
                order.Confirm();
                order.Ship();
                order.Deliver();
                break;

            case OrderStatus.Failed:
                order.MarkAsFailed("Test failure");
                break;

            default:
                throw new ArgumentException($"Unsupported target status: {targetStatus}");
        }

        order.ClearDomainEvents();
        return order;
    }

    private static ShippingAddress CreateValidAddress()
    {
        return new ShippingAddress(
            "John Doe",
            "john@example.com",
            "123 Main St",
            "San Francisco",
            "CA",
            "94102");
    }
}
