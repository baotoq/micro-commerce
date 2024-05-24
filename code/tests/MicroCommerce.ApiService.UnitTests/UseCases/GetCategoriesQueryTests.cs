using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.UseCases.Categories;

namespace MicroCommerce.ApiService.UnitTests.UseCases;
public class GetCategoriesQueryTests : TestBase
{
    [Fact]
    public async Task Verify_All_Categories_Are_Retrieved()
    {
        // Arrange
        var categoryList = new List<Category>
        {
            new() { Id = "1", Name = "Buccaneers" },
            new() { Id = "2", Name = "Corsairs" }
        };
        await Context.Categories.AddRangeAsync(categoryList);
        await Context.SaveChangesAsync();
        
        // Act
        var handler = new GetCategoriesQuery.Handler(Context);
        var query = new GetCategoriesQuery();
        
        // Assert
        var result = await handler.Handle(query, default);
        
        var categoryViewModels = result.ToList();
        categoryViewModels.Should().HaveCount(2);
        categoryViewModels.Should().ContainEquivalentOf(new CategoryViewModel { Id = "1", Name = "Buccaneers" });
        categoryViewModels.Should().ContainEquivalentOf(new CategoryViewModel { Id = "2", Name = "Corsairs" });
    }
    
    [Fact]
    public async Task Test_Response_When_Database_Is_Empty()
    {
        // Arrange
        var handler = new GetCategoriesQuery.Handler(Context);
        var query = new GetCategoriesQuery();
        
        // Act
        var result = await handler.Handle(query, default);
        
        // Assert
        result.Should().BeEmpty();
    }
}