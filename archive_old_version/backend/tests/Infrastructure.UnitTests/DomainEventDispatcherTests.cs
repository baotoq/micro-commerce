using Domain;
using Infrastructure.UnitTests.Persistence.Interceptors;
using MassTransit;
using NSubstitute;
using Xunit;

namespace Infrastructure.UnitTests;

public class MassTransitDomainEventDispatcherTests
{
    private readonly MassTransitDomainEventDispatcher _sut;
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitDomainEventDispatcherTests()
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _sut = new MassTransitDomainEventDispatcher(_publishEndpoint);
    }

    [Fact]
    public async Task ShouldCallPublishEvents()
    {
        var domainEvents = new List<IDomainEvent>()
        {
            new TestEntityCreatedDomainEvent(),
            new TestEntityUpdatedDomainEvent(),
            new TestEntityUpdatedDomainEvent()
        };
        
        await _sut.DispatchAsync(domainEvents);

        await _publishEndpoint.Received(3).Publish(Arg.Any<IDomainEvent>());
    }
}