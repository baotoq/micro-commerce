using FluentAssertions;
using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.Events;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Catalog.Aggregates;

[Trait("Category", "Unit")]
public sealed class ProductTests
{
    [Fact]
    public void Create_ValidData_ReturnsProductWithDraftStatus()
    {
        // Arrange
        ProductName name = ProductName.Create("Test Product");
        string description = "Test Description";
        Money price = Money.Create(99.99m);
        CategoryId categoryId = CategoryId.New();

        // Act
        Product product = Product.Create(name, description, price, categoryId);

        // Assert
        product.Status.Should().Be(ProductStatus.Draft);
        product.Id.Should().NotBeNull();
        product.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ValidData_RaisesProductCreatedDomainEvent()
    {
        // Arrange
        ProductName name = ProductName.Create("Test Product");
        Money price = Money.Create(99.99m);

        // Act
        Product product = Product.Create(name, "Description", price, CategoryId.New());

        // Assert
        product.DomainEvents.Should().HaveCount(1);
        product.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Create_ValidData_SetsAllProperties()
    {
        // Arrange
        ProductName name = ProductName.Create("Test Product");
        string description = "Test Description";
        Money price = Money.Create(99.99m);
        CategoryId categoryId = CategoryId.New();
        string imageUrl = "https://example.com/image.jpg";
        string sku = "SKU-123";

        // Act
        Product product = Product.Create(name, description, price, categoryId, imageUrl, sku);

        // Assert
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.CategoryId.Should().Be(categoryId);
        product.ImageUrl.Should().Be(imageUrl);
        product.Sku.Should().Be(sku);
        product.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        product.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Update_ValidData_UpdatesProperties()
    {
        // Arrange
        Product product = CreateValidProduct();
        ProductName newName = ProductName.Create("Updated Product");
        string newDescription = "Updated Description";
        Money newPrice = Money.Create(149.99m);
        CategoryId newCategoryId = CategoryId.New();
        string newImageUrl = "https://example.com/new-image.jpg";
        string newSku = "SKU-456";

        // Act
        product.Update(newName, newDescription, newPrice, newCategoryId, newImageUrl, newSku);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
        product.CategoryId.Should().Be(newCategoryId);
        product.ImageUrl.Should().Be(newImageUrl);
        product.Sku.Should().Be(newSku);
    }

    [Fact]
    public void Update_ValidData_RaisesProductUpdatedDomainEvent()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.ClearDomainEvents();

        // Act
        product.Update(
            ProductName.Create("New Name"),
            "New Description",
            Money.Create(50m),
            CategoryId.New(),
            null,
            null);

        // Assert
        product.DomainEvents.Should().ContainSingle(e => e is ProductUpdatedDomainEvent);
    }

    [Fact]
    public void Update_ValidData_SetsUpdatedAt()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.UpdatedAt.Should().BeNull();

        // Act
        product.Update(
            ProductName.Create("New Name"),
            "New Description",
            Money.Create(50m),
            CategoryId.New(),
            null,
            null);

        // Assert
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Publish_WhenDraft_ChangesToPublished()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Status.Should().Be(ProductStatus.Draft);

        // Act
        product.Publish();

        // Assert
        product.Status.Should().Be(ProductStatus.Published);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_NoOp()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Publish();
        product.ClearDomainEvents();
        ProductStatus statusBeforeSecondPublish = product.Status;

        // Act
        product.Publish();

        // Assert
        product.Status.Should().Be(statusBeforeSecondPublish);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Publish_SetsUpdatedAt()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.UpdatedAt.Should().BeNull();

        // Act
        product.Publish();

        // Assert
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Unpublish_WhenPublished_ChangesToDraft()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Publish();
        product.Status.Should().Be(ProductStatus.Published);

        // Act
        product.Unpublish();

        // Assert
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Unpublish_WhenDraft_NoOp()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Status.Should().Be(ProductStatus.Draft);
        product.ClearDomainEvents();

        // Act
        product.Unpublish();

        // Assert
        product.Status.Should().Be(ProductStatus.Draft);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Archive_WhenPublished_ChangesToArchived()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Publish();
        product.Status.Should().Be(ProductStatus.Published);

        // Act
        product.Archive();

        // Assert
        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Archive_WhenDraft_ChangesToArchived()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Status.Should().Be(ProductStatus.Draft);

        // Act
        product.Archive();

        // Assert
        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Archive_WhenArchived_NoOp()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.Archive();
        product.Status.Should().Be(ProductStatus.Archived);
        product.ClearDomainEvents();

        // Act
        product.Archive();

        // Assert
        product.Status.Should().Be(ProductStatus.Archived);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Archive_RaisesProductArchivedDomainEvent()
    {
        // Arrange
        Product product = CreateValidProduct();
        product.ClearDomainEvents();

        // Act
        product.Archive();

        // Assert
        product.DomainEvents.Should().ContainSingle(e => e is ProductArchivedDomainEvent);
    }

    // Helper methods

    private static Product CreateValidProduct()
    {
        return Product.Create(
            ProductName.Create("Test Product"),
            "Test Description",
            Money.Create(99.99m),
            CategoryId.New(),
            "https://example.com/image.jpg",
            "SKU-123");
    }
}
