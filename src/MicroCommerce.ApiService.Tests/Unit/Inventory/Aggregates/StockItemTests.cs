using FluentAssertions;
using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using MicroCommerce.ApiService.Features.Inventory.Domain.Events;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Inventory.Aggregates;

[Trait("Category", "Unit")]
public sealed class StockItemTests
{
    [Fact]
    public void Create_ValidProductId_ReturnsStockItemWithZeroQuantity()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        // Act
        StockItem stockItem = StockItem.Create(productId);

        // Assert
        stockItem.ProductId.Should().Be(productId);
        stockItem.QuantityOnHand.Should().Be(0);
        stockItem.Reservations.Should().BeEmpty();
        stockItem.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public void AdjustStock_PositiveAdjustment_IncreasesQuantity()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 10);

        // Act
        stockItem.AdjustStock(5);

        // Assert
        stockItem.QuantityOnHand.Should().Be(15);
    }

    [Fact]
    public void AdjustStock_NegativeAdjustment_DecreasesQuantity()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 10);

        // Act
        stockItem.AdjustStock(-3);

        // Assert
        stockItem.QuantityOnHand.Should().Be(7);
    }

    [Fact]
    public void AdjustStock_WouldGoNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 5);

        // Act
        Action act = () => stockItem.AdjustStock(-10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*negative quantity*");
    }

    [Fact]
    public void AdjustStock_RaisesStockAdjustedDomainEvent()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.ClearDomainEvents();

        // Act
        stockItem.AdjustStock(5);

        // Assert
        stockItem.DomainEvents.Should().ContainSingle(e => e is StockAdjustedDomainEvent);
        StockAdjustedDomainEvent domainEvent = stockItem.DomainEvents.OfType<StockAdjustedDomainEvent>().First();
        domainEvent.Adjustment.Should().Be(5);
        domainEvent.NewQuantity.Should().Be(15);
    }

    [Fact]
    public void AdjustStock_ResultBelow10_RaisesStockLowDomainEvent()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 15);
        stockItem.ClearDomainEvents();

        // Act
        stockItem.AdjustStock(-7); // Results in 8

        // Assert
        stockItem.DomainEvents.Should().Contain(e => e is StockLowDomainEvent);
    }

    [Fact]
    public void AdjustStock_ResultAbove10_DoesNotRaiseStockLowEvent()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 10);
        stockItem.ClearDomainEvents();

        // Act
        stockItem.AdjustStock(5); // Results in 15

        // Assert
        stockItem.DomainEvents.Should().NotContain(e => e is StockLowDomainEvent);
    }

    [Fact]
    public void Reserve_SufficientStock_ReturnsReservationId()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 20);

        // Act
        ReservationId reservationId = stockItem.Reserve(5);

        // Assert
        reservationId.Should().NotBeNull();
        stockItem.Reservations.Should().HaveCount(1);
        stockItem.AvailableQuantity.Should().Be(15); // 20 - 5
    }

    [Fact]
    public void Reserve_InsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 5);

        // Act
        Action act = () => stockItem.Reserve(10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient available stock*");
    }

    [Fact]
    public void Reserve_ZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 10);

        // Act
        Action act = () => stockItem.Reserve(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void Reserve_NegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 10);

        // Act
        Action act = () => stockItem.Reserve(-5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void Reserve_AccountsForExistingReservations()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 20);
        stockItem.Reserve(5); // First reservation: 5 units

        // Act
        ReservationId secondReservation = stockItem.Reserve(10); // Try to reserve 10 more

        // Assert
        secondReservation.Should().NotBeNull();
        stockItem.Reservations.Should().HaveCount(2);
        stockItem.AvailableQuantity.Should().Be(5); // 20 - 5 - 10
    }

    [Fact]
    public void ReleaseReservation_ExistingReservation_RemovesFromCollection()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 20);
        ReservationId reservationId = stockItem.Reserve(5);
        stockItem.Reservations.Should().HaveCount(1);

        // Act
        stockItem.ReleaseReservation(reservationId);

        // Assert
        stockItem.Reservations.Should().BeEmpty();
        stockItem.AvailableQuantity.Should().Be(20); // Back to original
    }

    [Fact]
    public void ReleaseReservation_NonExistent_NoOp()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 20);
        stockItem.Reserve(5);
        int originalReservationCount = stockItem.Reservations.Count;
        ReservationId nonExistentReservationId = ReservationId.New();

        // Act
        stockItem.ReleaseReservation(nonExistentReservationId);

        // Assert
        stockItem.Reservations.Should().HaveCount(originalReservationCount); // No change
    }

    [Fact]
    public void AvailableQuantity_NoReservations_EqualsQuantityOnHand()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 100);

        // Assert
        stockItem.AvailableQuantity.Should().Be(100);
    }

    [Fact]
    public void AvailableQuantity_WithReservations_SubtractsReservedQuantity()
    {
        // Arrange
        StockItem stockItem = CreateStockItem(initialQuantity: 100);
        stockItem.Reserve(20);
        stockItem.Reserve(15);

        // Assert
        stockItem.AvailableQuantity.Should().Be(65); // 100 - 20 - 15
    }

    // Helper methods

    private static StockItem CreateStockItem(int initialQuantity = 0)
    {
        StockItem stockItem = StockItem.Create(Guid.NewGuid());
        if (initialQuantity > 0)
        {
            stockItem.AdjustStock(initialQuantity);
            stockItem.ClearDomainEvents();
        }
        return stockItem;
    }
}
