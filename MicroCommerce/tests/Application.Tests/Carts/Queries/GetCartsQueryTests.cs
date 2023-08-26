using Application.Carts.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Carts.Queries;

public class GetCartsQueryTests
{
    private readonly GetCartsQueryHandler _sut;
    private readonly ApplicationDbContext _context;

    public GetCartsQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _sut = new GetCartsQueryHandler(_context);
    }
    
    [Fact]
    public async Task GetCarts()
    {
        _context.Carts.Add(new Cart());
        await _context.SaveChangesAsync();

        var act = await _sut.Handle(new GetCartsQuery(), default);

        act.Data.Should().HaveCount(1);
    }
}