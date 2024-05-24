using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.UnitTests;
using MicroCommerce.ApiService.UseCases.Categories;

namespace MicroCommerce.ApiService.UnitTests;
public class GetCategoryQueryTests : TestBase
{
    [Fact]
    public async Task ValidCategoryIdReturnsViewModelUsingInMemoryDb()
    {
        // Arrange
        Context.Categories.Add(new Category { Id = "1", Name = "Electronics" });
        Context.SaveChanges();

        var handler = new GetCategoryQuery.Handler(Context);
        var query = new GetCategoryQuery("1");

        // Act
        var result = await handler.Handle(query, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CategoryViewModel>();
    }
}