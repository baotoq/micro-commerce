using Application.UseCases.Carts.Queries;
using Domain.Entities;
using FluentAssertions;

namespace Application.UnitTests.UseCases.Carts.Queries;

public class GetCartsQueryTests : TestBase
{
    private readonly GetCartsQuery.Handler _sut;

    public GetCartsQueryTests()
    {
        _sut = new GetCartsQuery.Handler(Context, Mapper);
    }
    
    [Fact]
    public async Task GetCarts()
    {
        Context.Carts.Add(new Cart());
        await Context.SaveChangesAsync();

        var act = await _sut.Handle(new GetCartsQuery());

        act.Data.Should().HaveCount(1);
    }
}