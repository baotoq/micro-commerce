using Application.Common.AutoMapper;
using AutoMapper;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.UnitTests;

public class TestBase
{
    protected readonly ApplicationDbContext Context;
    protected readonly IMapper Mapper;
    protected readonly IPublishEndpoint PublishEndpoint;

    protected TestBase()
    {
        PublishEndpoint = Substitute.For<IPublishEndpoint>();
        var domainEventDispatcher = Substitute.For<MassTransitDomainEventDispatcher>(PublishEndpoint);
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new DateEntityInterceptor())
            .AddInterceptors(new DispatchDomainEventsInterceptor(domainEventDispatcher))
            .Options;
        
        Context = new ApplicationDbContext(options);
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        Mapper = config.CreateMapper();
    }
}