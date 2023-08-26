using Application.UseCases.Products.Commands;
using FluentAssertions;

namespace Application.Tests.UseCases.Products.Commands;

public class CreateProductCommandTests : TestBase
{
    private CreateProductCommand.Handler _sut;

    public CreateProductCommandTests()
    {
        _sut = new CreateProductCommand.Handler(Context);
    }

    [Fact]
    public async Task Create()
    {
        var act = await _sut.Handle(new CreateProductCommand
        {
            Name = "Apple"
        });

        act.Name.Should().Be("Apple");
    }
}