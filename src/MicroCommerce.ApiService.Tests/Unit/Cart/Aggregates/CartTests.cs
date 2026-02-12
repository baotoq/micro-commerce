using FluentAssertions;
using MicroCommerce.ApiService.Features.Cart.Domain.Entities;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Cart.Aggregates;

[Trait("Category", "Unit")]
public sealed class CartTests
{
    [Fact]
    public void Create_ValidBuyerId_ReturnsCartWithEmptyItems()
    {
        // Arrange
        Guid buyerId = Guid.NewGuid();

        // Act
        Domain.Entities.Cart cart = Domain.Entities.Cart.Create(buyerId);

        // Assert
        cart.BuyerId.Should().Be(buyerId);
        cart.Items.Should().BeEmpty();
        cart.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        cart.LastModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_SetsExpiresAt30DaysFromNow()
    {
        // Arrange
        Guid buyerId = Guid.NewGuid();
        DateTimeOffset expectedExpiry = DateTimeOffset.UtcNow.AddDays(30);

        // Act
        Domain.Entities.Cart cart = Domain.Entities.Cart.Create(buyerId);

        // Assert
        cart.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddItem_NewProduct_AddsToItems()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        Guid productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, "Test Product", 99.99m, "https://example.com/image.jpg", 1);

        // Assert
        cart.Items.Should().HaveCount(1);
        CartItem item = cart.Items.First();
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Test Product");
        item.UnitPrice.Should().Be(99.99m);
        item.Quantity.Should().Be(1);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncrementsQuantity()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        Guid productId = Guid.NewGuid();
        cart.AddItem(productId, "Test Product", 99.99m, null, 2);

        // Act
        cart.AddItem(productId, "Test Product", 99.99m, null, 3);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_QuantityExceeds99_CappedAt99()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        Guid productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, "Test Product", 99.99m, null, 150);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(99);
    }

    [Fact]
    public void AddItem_ZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();

        // Act
        Action act = () => cart.AddItem(Guid.NewGuid(), "Test Product", 99.99m, null, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity")
            .WithMessage("*Quantity must be at least 1*");
    }

    [Fact]
    public void AddItem_UpdatesLastModifiedAt()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        DateTimeOffset originalLastModified = cart.LastModifiedAt;

        // Wait a bit to ensure time difference
        System.Threading.Thread.Sleep(10);

        // Act
        cart.AddItem(Guid.NewGuid(), "Test Product", 99.99m, null, 1);

        // Assert
        cart.LastModifiedAt.Should().BeAfter(originalLastModified);
    }

    [Fact]
    public void AddItem_ResetsExpiresAt()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        DateTimeOffset originalExpiresAt = cart.ExpiresAt;
        DateTimeOffset expectedNewExpiry = DateTimeOffset.UtcNow.AddDays(30);

        // Wait a bit to ensure time difference
        System.Threading.Thread.Sleep(10);

        // Act
        cart.AddItem(Guid.NewGuid(), "Test Product", 99.99m, null, 1);

        // Assert
        cart.ExpiresAt.Should().BeAfter(originalExpiresAt);
        cart.ExpiresAt.Should().BeCloseTo(expectedNewExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateItemQuantity_ValidQuantity_UpdatesItem()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        Guid productId = Guid.NewGuid();
        cart.AddItem(productId, "Test Product", 99.99m, null, 1);
        CartItemId itemId = cart.Items.First().Id;

        // Act
        cart.UpdateItemQuantity(itemId, 5);

        // Assert
        cart.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_Exceeds99_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        cart.AddItem(Guid.NewGuid(), "Test Product", 99.99m, null, 1);
        CartItemId itemId = cart.Items.First().Id;

        // Act
        Action act = () => cart.UpdateItemQuantity(itemId, 100);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("newQuantity")
            .WithMessage("*Quantity must be between 1 and 99*");
    }

    [Fact]
    public void UpdateItemQuantity_Zero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        cart.AddItem(Guid.NewGuid(), "Test Product", 99.99m, null, 5);
        CartItemId itemId = cart.Items.First().Id;

        // Act
        Action act = () => cart.UpdateItemQuantity(itemId, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("newQuantity");
    }

    [Fact]
    public void UpdateItemQuantity_InvalidItemId_ThrowsInvalidOperationException()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        CartItemId nonExistentItemId = CartItemId.New();

        // Act
        Action act = () => cart.UpdateItemQuantity(nonExistentItemId, 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Cart item '{nonExistentItemId}' not found.");
    }

    [Fact]
    public void RemoveItem_ExistingItem_RemovesFromCollection()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        cart.AddItem(Guid.NewGuid(), "Product 1", 10.00m, null, 1);
        cart.AddItem(Guid.NewGuid(), "Product 2", 20.00m, null, 1);
        cart.Items.Should().HaveCount(2);
        CartItemId itemIdToRemove = cart.Items.First().Id;

        // Act
        cart.RemoveItem(itemIdToRemove);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.Should().NotContain(i => i.Id == itemIdToRemove);
    }

    [Fact]
    public void RemoveItem_NonExistentItem_NoOp()
    {
        // Arrange
        Domain.Entities.Cart cart = CreateValidCart();
        cart.AddItem(Guid.NewGuid(), "Product", 10.00m, null, 1);
        int originalCount = cart.Items.Count;
        CartItemId nonExistentItemId = CartItemId.New();

        // Act
        cart.RemoveItem(nonExistentItemId);

        // Assert
        cart.Items.Should().HaveCount(originalCount);
    }

    // Helper methods

    private static Domain.Entities.Cart CreateValidCart()
    {
        return Domain.Entities.Cart.Create(Guid.NewGuid());
    }
}
