using Application.UseCases.Products.Commands;
using Application.UseCases.Products.DomainEvents;
using FluentAssertions;
using MassTransit;
using NSubstitute;

namespace Application.Tests.UseCases.Products.Commands;

public class CreateProductCommandTests : TestBase
{
    private readonly CreateProductCommand.Handler _sut;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateProductCommandTests()
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _sut = new CreateProductCommand.Handler(Context, _publishEndpoint);
    }

    [Fact]
    public async Task Create()
    {
        var act = await _sut.Handle(new CreateProductCommand
        {
            Name = "Apple"
        });
        
        await _publishEndpoint.Received().Publish(Arg.Any<ProductCreatedDomainEvent>());
        act.Name.Should().Be("Apple");
    }
}