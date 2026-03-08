using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetCategories;
using MicroCommerce.ApiService.Features.Catalog.Application.Queries.GetProducts;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Catalog;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class CatalogEndpointsTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    public CatalogEndpointsTests(ApiWebApplicationFactory factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(CatalogDbContext));
    }

    [Fact]
    public async Task GetProducts_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        ProductListDto? response = await _client.GetFromJsonAsync<ProductListDto>("/api/catalog/products");

        // Assert
        response.Should().NotBeNull();
        response!.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateCategory_ValidRequest_Returns201()
    {
        // Arrange
        CreateCategoryRequest request = new("Electronics", "Electronic devices and accessories");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/catalog/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateCategoryResponse? result = await response.Content.ReadFromJsonAsync<CreateCategoryResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCategories_ReturnsCategories()
    {
        // Arrange - Create a category first
        CreateCategoryRequest request = new("Books", "Physical and digital books");
        await _client.PostAsJsonAsync("/api/catalog/categories", request);

        // Act
        List<CategoryDto>? categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/catalog/categories");

        // Assert
        categories.Should().NotBeNull();
        categories.Should().HaveCountGreaterThanOrEqualTo(1);
        categories.Should().Contain(c => c.Name == "Books");
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201()
    {
        // Arrange - Create category first
        CreateCategoryRequest categoryRequest = new("Laptops", "Laptop computers");
        HttpResponseMessage categoryResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? category = await categoryResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        CreateProductRequest productRequest = new(
            "MacBook Pro",
            "Apple MacBook Pro with M2 chip",
            2499.99m,
            category!.Id,
            "https://example.com/macbook.jpg",
            "MBP-M2-2023");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/catalog/products", productRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateProductResponse? result = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_Returns400()
    {
        // Arrange - Missing required fields (empty name)
        CreateProductRequest request = new(
            "",
            "Description",
            100m,
            Guid.NewGuid(),
            null,
            null);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/catalog/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_ExistingProduct_ReturnsProduct()
    {
        // Arrange - Create category and product
        CreateCategoryRequest categoryRequest = new("Smartphones", "Mobile phones");
        HttpResponseMessage categoryResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? category = await categoryResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        CreateProductRequest productRequest = new(
            "iPhone 15",
            "Latest iPhone model",
            999.99m,
            category!.Id,
            null,
            "IPH15");

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/products", productRequest);
        CreateProductResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();

        // Act
        ProductDto? product = await _client.GetFromJsonAsync<ProductDto>($"/api/catalog/products/{created!.Id}");

        // Assert
        product.Should().NotBeNull();
        product!.Name.Should().Be("iPhone 15");
        product.Price.Should().Be(999.99m);
    }

    [Fact]
    public async Task GetProductById_NonExistent_Returns404()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/catalog/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_ValidRequest_ReturnsNoContent()
    {
        // Arrange - Create category and product
        CreateCategoryRequest categoryRequest = new("Tablets", "Tablet computers");
        HttpResponseMessage categoryResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? category = await categoryResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        CreateProductRequest productRequest = new(
            "iPad Pro",
            "Apple iPad Pro",
            1099.99m,
            category!.Id,
            null,
            "IPAD-PRO");

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/products", productRequest);
        CreateProductResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();

        UpdateProductRequest updateRequest = new(
            "iPad Pro 12.9\"",
            "Apple iPad Pro 12.9-inch with M2 chip",
            1199.99m,
            category.Id,
            "https://example.com/ipad-pro.jpg",
            "IPAD-PRO-12");

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/catalog/products/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        ProductDto? updated = await _client.GetFromJsonAsync<ProductDto>($"/api/catalog/products/{created.Id}");
        updated!.Name.Should().Be("iPad Pro 12.9\"");
        updated.Price.Should().Be(1199.99m);
    }

    [Fact]
    public async Task ChangeProductStatus_PublishProduct_ReturnsNoContent()
    {
        // Arrange - Create product (starts as Draft)
        CreateCategoryRequest categoryRequest = new("Cameras", "Camera equipment");
        HttpResponseMessage categoryResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? category = await categoryResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        CreateProductRequest productRequest = new(
            "Canon EOS R5",
            "Full-frame mirrorless camera",
            3899.99m,
            category!.Id,
            null,
            "CANON-R5");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/products", productRequest);
        CreateProductResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();

        // Act - Publish the product
        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/catalog/products/{created!.Id}/status",
            new { Status = "Published" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeProductStatus_InvalidTransition_Returns422()
    {
        // Arrange - Create product (starts as Draft); can't go directly to Archived
        CreateCategoryRequest categoryRequest = new("Audio", "Audio equipment");
        HttpResponseMessage categoryResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? category = await categoryResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        CreateProductRequest productRequest = new(
            "Sony Headphones",
            "Noise-cancelling headphones",
            349.99m,
            category!.Id,
            null,
            "SONY-WH");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/products", productRequest);
        CreateProductResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();

        // Act - Try invalid transition: Draft -> Archived
        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/catalog/products/{created!.Id}/status",
            new { Status = "Archived" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ArchiveProduct_PublishedProduct_ReturnsNoContent()
    {
        // Arrange - Create and publish product first
        CreateCategoryRequest categoryRequest = new("Wearables", "Wearable technology");
        HttpResponseMessage categoryResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? category = await categoryResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        CreateProductRequest productRequest = new(
            "Apple Watch",
            "Smartwatch",
            399.99m,
            category!.Id,
            null,
            "AW-S9");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/products", productRequest);
        CreateProductResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();

        // Publish first (Draft -> Published)
        await _client.PatchAsJsonAsync(
            $"/api/catalog/products/{created!.Id}/status",
            new { Status = "Published" });

        // Act - Archive (Published -> Archived via DELETE)
        HttpResponseMessage response = await _client.DeleteAsync($"/api/catalog/products/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ArchiveProduct_NonExistentProduct_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/catalog/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadProductImage_ValidImage_ReturnsCreated()
    {
        // Arrange
        byte[] fakePng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        using MultipartFormDataContent content = new();
        using ByteArrayContent imageContent = new(fakePng);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "file", "product.png");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/catalog/images", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        string body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("imageUrl");
    }

    [Fact]
    public async Task GetCategoryById_ExistingCategory_ReturnsCategory()
    {
        // Arrange
        CreateCategoryRequest categoryRequest = new("Gaming", "Gaming peripherals");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/catalog/categories/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Gaming");
    }

    [Fact]
    public async Task GetCategoryById_NonExistentCategory_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/catalog/categories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_ExistingCategory_ReturnsNoContent()
    {
        // Arrange
        CreateCategoryRequest categoryRequest = new("Fitness", "Fitness equipment");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/catalog/categories/{created!.Id}",
            new { Name = "Fitness & Wellness", Description = "Fitness and wellness products" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCategory_ExistingEmptyCategory_ReturnsNoContent()
    {
        // Arrange - Create category without products
        CreateCategoryRequest categoryRequest = new("Temporary", "Temp category");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog/categories", categoryRequest);
        CreateCategoryResponse? created = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/catalog/categories/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
