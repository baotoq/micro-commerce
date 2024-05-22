using AutoFixture;
using Bogus;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.UseCases.Categories;
using MicroCommerce.ApiService.UseCases.Products;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.ApiService.UnitTests;

public class DeleteCategoryCommandTests : TestBase
{
    [Fact]
    public async Task DeleteCategory()
    {
        // Arrange
        var fixture = new Fixture();
        var category = fixture.Build<Category>()
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