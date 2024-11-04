using AutoFixture;
using MicroCommerce.ApiService.Domain.Entities;

namespace MicroCommerce.ApiService.UnitTests.UseCases;

public class UpdateCategoryCommandTests : TestBase
{
    private readonly Fixture _categoryFixture = new();

    [Fact]
    public async Task Category_Is_Successfully_Updated()
    {
        // Arrange
        await Context.Categories.AddAsync(new Category { Id = "1", Name = "Original Category" });
        await Context.SaveChangesAsync();

        var handler = new UpdateCategoryCommand.Handler(Context);
        var command = new UpdateCategoryCommand { Id = "1", Name = "Updated Category" };

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().Be("1");
    }
}