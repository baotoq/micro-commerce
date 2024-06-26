using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.UseCases.Products;

namespace MicroCommerce.ApiService.UnitTests.UseCases;

public class GetProductQueryTests : TestBase
{
    [Fact]
    public async Task GetProductById()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Apple"
        };
        Context.AddRange(product);

        await Context.SaveChangesAsync();
        
        var request = new GetProductQuery(product.Id);
        var handler = new GetProductQueryHandler(Context);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.Name.Should().Be("Apple");
    }
    
    [Fact]
    public async Task GetProductById_NotFound()
    {
        // Arrange
        var request = new GetProductQuery(Guid.NewGuid().ToString());
        var handler = new GetProductQueryHandler(Context);

        // Act
        var result = async () => await handler.Handle(request, default);

        // Assert
        await result.Should().ThrowAsync<Exception>();
    }
}