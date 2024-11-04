namespace MicroCommerce.ApiService.UnitTests.UseCases;

public class CreateCategoryCommandTests : TestBase
{
    [Fact]
    public async Task Valid_Category_Name_Returns_Category_Id()
    {
        // Arrange
        var handler = new CreateCategoryCommand.Handler(Context);
        var command = new CreateCategoryCommand { Name = "Electronics" };
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        result.Should().NotBeNullOrEmpty();
        Context.Categories.Count().Should().Be(1);
    }
}