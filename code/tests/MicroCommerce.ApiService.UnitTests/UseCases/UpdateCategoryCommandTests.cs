using AutoFixture;
using Bogus;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.UseCases.Categories;
using MicroCommerce.ApiService.UseCases.Products;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.ApiService.UnitTests;

public class UpdateCategoryCommandTests : TestBase
{
    private readonly Fixture _categoryFixture = new();

    [Fact]
    public async Task Category_Is_Successfully_Updated()
    {
        // Arrange
        Context.Categories.Add(new Category { Id = "1", Name = "Original Category" });
        Context.SaveChanges();

        var handler = new UpdateCategoryCommand.Handler(Context);
        var command = new UpdateCategoryCommand { Id = "1", Name = "Updated Category" };

        // Act
        var result = await handler.Handle(command, new CancellationToken());

        // Assert
        result.Should().Be("1");
    }
}