using Application.UseCases.Products.Queries;
using Domain.Entities;
using FluentAssertions;

namespace Application.UnitTests.UseCases.Products.Queries;

public class GetProductsQueryTests : TestBase
{
    private readonly GetProductsQuery.Handler _sut;

    public GetProductsQueryTests()
    {
        _sut = new GetProductsQuery.Handler(Context, Mapper);
    }
    
    [Fact]
    public async Task GetProducts()
    {
        Context.Products.Add(new Product());
        await Context.SaveChangesAsync();

        var act = await _sut.Handle(new GetProductsQuery());

        act.Data.Should().HaveCount(1);
    }
}