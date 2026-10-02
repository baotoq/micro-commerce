using MicroCommerce.ApiService.Features.Products;

namespace MicroCommerce.UnitTests.Features.Products;

public class ProductTests
{
    [Fact]
    public void New_product_gets_a_version_7_id()
    {
        var product = new Product { Name = "Coffee Mug" };

        product.Id.Version.ShouldBe(7);
    }

    [Fact]
    public void Response_maps_all_fields()
    {
        var product = new Product { Name = "Coffee Mug", Price = 12.5m };

        var response = product.ToResponse();

        response.ShouldBe(new ProductResponse(product.Id, "Coffee Mug", 12.5m, product.CreatedAt));
    }
}
