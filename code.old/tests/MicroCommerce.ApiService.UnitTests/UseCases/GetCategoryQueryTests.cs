using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.UseCases.Categories;

namespace MicroCommerce.ApiService.UnitTests.UseCases;
public class GetCategoryQueryTests : TestBase
{
    [Fact]
    public async Task Valid_Category_Id_Returns_ViewModel()
    {
        // Arrange
        await Context.Categories.AddAsync(new Category { Id = "1", Name = "Electronics" });
        await Context.SaveChangesAsync();

        var handler = new GetCategoryQuery.Handler(Context);
        var query = new GetCategoryQuery("1");

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CategoryViewModel>();
    }
}