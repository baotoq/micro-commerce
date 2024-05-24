using AutoFixture;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.UseCases.Categories;

namespace MicroCommerce.ApiService.UnitTests.UseCases;

public class DeleteCategoryCommandTests : TestBase
{
    private readonly Fixture _categoryFixture = new();
    
    [Fact]
    public async Task DeleteCategory()
    {
        // Arrange
        var category = _categoryFixture.Build<Category>()
            .Without(s => s.CreatedAt)
            .Without(s => s.UpdatedAt)
            .Create();
        
        await Context.Categories.AddAsync(category);
        await Context.SaveChangesAsync();
        
        var request = new DeleteCategoryCommand(category.Id);
        
        var handler = new DeleteCategoryCommand.Handler(Context);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}