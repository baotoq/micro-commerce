using Application.Common.AutoMapper;
using AutoMapper;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.Tests;

public class TestBase
{
    protected readonly ApplicationDbContext Context;
    protected readonly IMapper Mapper;

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        Context = new ApplicationDbContext(options);
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        Mapper = config.CreateMapper();
    }
}