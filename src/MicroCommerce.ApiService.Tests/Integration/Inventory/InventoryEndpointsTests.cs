using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Inventory;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetAdjustmentHistory;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;
using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.ApiService.Tests.Integration.Inventory;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class InventoryEndpointsTests
{
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly InventoryDbContext _dbContext;

    public InventoryEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    }

    [Fact]
    public async Task GetStockLevels_ExistingProductIds_ReturnsStockInfo()
    {
        // Arrange - Seed stock items directly in database
        Guid product1 = Guid.NewGuid();
        Guid product2 = Guid.NewGuid();

        StockItem stock1 = StockItem.Create(product1);
        stock1.AdjustStock(100, "Initial stock");
        StockItem stock2 = StockItem.Create(product2);
        stock2.AdjustStock(50, "Initial stock");

        _dbContext.StockItems.AddRange(stock1, stock2);
        await _dbContext.SaveChangesAsync();

        // Act
        List<StockInfoDto>? stockLevels = await _client.GetFromJsonAsync<List<StockInfoDto>>(
            $"/api/inventory/stock?productIds={product1},{product2}");

        // Assert
        stockLevels.Should().NotBeNull();
        stockLevels.Should().HaveCount(2);
        stockLevels.Should().Contain(s => s.ProductId == product1 && s.AvailableQuantity == 100);
        stockLevels.Should().Contain(s => s.ProductId == product2 && s.AvailableQuantity == 50);
    }

    [Fact]
    public async Task GetStockLevels_MissingProductIds_ReturnsZeroStock()
    {
        // Arrange
        Guid nonExistentProduct = Guid.NewGuid();

        // Act
        List<StockInfoDto>? stockLevels = await _client.GetFromJsonAsync<List<StockInfoDto>>(
            $"/api/inventory/stock?productIds={nonExistentProduct}");

        // Assert
        stockLevels.Should().NotBeNull();
        stockLevels.Should().HaveCount(1);
        stockLevels![0].ProductId.Should().Be(nonExistentProduct);
        stockLevels[0].AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public async Task AdjustStock_ValidAdjustment_ReturnsNoContent()
    {
        // Arrange - Seed stock item
        Guid productId = Guid.NewGuid();
        StockItem stock = StockItem.Create(productId);
        stock.AdjustStock(50, "Initial stock");
        _dbContext.StockItems.Add(stock);
        await _dbContext.SaveChangesAsync();

        AdjustStockRequest request = new(
            Adjustment: 10,
            Reason: "Restock");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/inventory/stock/{productId}/adjust", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify adjustment
        StockInfoDto? stockInfo = await _client.GetFromJsonAsync<StockInfoDto>($"/api/inventory/stock/{productId}");
        stockInfo!.AvailableQuantity.Should().Be(60); // 50 + 10
    }

    [Fact]
    public async Task AdjustStock_NonExistentProduct_Returns404()
    {
        // Arrange
        Guid nonExistentProduct = Guid.NewGuid();
        AdjustStockRequest request = new(
            Adjustment: 10,
            Reason: "Restock");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/inventory/stock/{nonExistentProduct}/adjust", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReserveStock_ValidRequest_Returns201()
    {
        // Arrange - Seed stock item
        Guid productId = Guid.NewGuid();
        StockItem stock = StockItem.Create(productId);
        stock.AdjustStock(100, "Initial stock");
        _dbContext.StockItems.Add(stock);
        await _dbContext.SaveChangesAsync();

        ReserveStockRequest request = new(Quantity: 5);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/inventory/stock/{productId}/reserve", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ReserveStockResponse? result = await response.Content.ReadFromJsonAsync<ReserveStockResponse>();
        result.Should().NotBeNull();
        result!.ReservationId.Should().NotBeEmpty();

        // Verify available quantity decreased
        StockInfoDto? stockInfo = await _client.GetFromJsonAsync<StockInfoDto>($"/api/inventory/stock/{productId}");
        stockInfo!.AvailableQuantity.Should().Be(95); // 100 - 5
    }

    [Fact]
    public async Task ReserveStock_InsufficientStock_Returns409()
    {
        // Arrange - Seed stock item with low quantity
        Guid productId = Guid.NewGuid();
        StockItem stock = StockItem.Create(productId);
        stock.AdjustStock(3, "Initial stock");
        _dbContext.StockItems.Add(stock);
        await _dbContext.SaveChangesAsync();

        ReserveStockRequest request = new(Quantity: 10);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/inventory/stock/{productId}/reserve", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetStockByProductId_ExistingProduct_ReturnsStockInfo()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        StockItem stock = StockItem.Create(productId);
        stock.AdjustStock(75, "Initial stock");
        _dbContext.StockItems.Add(stock);
        await _dbContext.SaveChangesAsync();

        // Act
        StockInfoDto? result = await _client.GetFromJsonAsync<StockInfoDto>($"/api/inventory/stock/{productId}");

        // Assert
        result.Should().NotBeNull();
        result!.ProductId.Should().Be(productId);
        result.AvailableQuantity.Should().Be(75);
    }

    [Fact]
    public async Task GetStockByProductId_NonExistentProduct_Returns404()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/inventory/stock/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAdjustmentHistory_AfterAdjustments_ReturnsHistory()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        StockItem stock = StockItem.Create(productId);
        stock.AdjustStock(100, "Initial stock", "admin");
        stock.AdjustStock(-10, "Damage write-off", "admin");
        _dbContext.StockItems.Add(stock);
        await _dbContext.SaveChangesAsync();

        // Act
        List<AdjustmentDto>? history = await _client.GetFromJsonAsync<List<AdjustmentDto>>(
            $"/api/inventory/stock/{productId}/adjustments");

        // Assert
        history.Should().NotBeNull();
        history.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAdjustmentHistory_NonExistentProduct_ReturnsEmptyList()
    {
        // Act
        List<AdjustmentDto>? history = await _client.GetFromJsonAsync<List<AdjustmentDto>>(
            $"/api/inventory/stock/{Guid.NewGuid()}/adjustments");

        // Assert
        history.Should().NotBeNull();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task ReleaseReservation_ExistingReservation_ReturnsNoContent()
    {
        // Arrange - Seed stock item and create a reservation via API
        Guid productId = Guid.NewGuid();
        StockItem stock = StockItem.Create(productId);
        stock.AdjustStock(50, "Initial stock");
        _dbContext.StockItems.Add(stock);
        await _dbContext.SaveChangesAsync();

        Guid stockItemId = stock.Id.Value;

        // Reserve stock to get a reservationId
        HttpResponseMessage reserveResponse = await _client.PostAsJsonAsync(
            $"/api/inventory/stock/{productId}/reserve", new ReserveStockRequest(Quantity: 5));
        ReserveStockResponse? reservation = await reserveResponse.Content.ReadFromJsonAsync<ReserveStockResponse>();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(
            $"/api/inventory/reservations/{reservation!.ReservationId}?stockItemId={stockItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify stock is released (available quantity back to 50)
        StockInfoDto? stockInfo = await _client.GetFromJsonAsync<StockInfoDto>($"/api/inventory/stock/{productId}");
        stockInfo!.AvailableQuantity.Should().Be(50);
    }
}
