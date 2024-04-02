using Application.UseCases.Products.Commands;
using Application.UseCases.Products.DomainEvents;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.UseCases.Products.Commands;

public class CreateProductCommandTests : TestBase
{
    private readonly CreateProductCommand.Handler _sut;

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

        await PublishEndpoint.Received().Publish(Arg.Any<ProductCreatedDomainEvent>());
        
        act.Name.Should().Be("Apple");
    }
}