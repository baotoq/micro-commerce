using MicroCommerce.ApiService.Features.Products;

namespace MicroCommerce.UnitTests.Features.Products;

public class ProductsMapperTests
{
    [Fact]
    public void Projection_maps_all_fields()
    {
        var product = new Product { Name = "Coffee Mug", Price = 12.5m };

        var responses = new[] { product }.AsQueryable().ProjectToResponse().ToList();

        responses.ShouldBe([new ProductResponse(product.Id, "Coffee Mug", 12.5m, product.CreatedAt)]);
    }
}
