using Application.Common.AutoMapper;
using Application.UseCases.Carts.Queries;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.UseCases.Carts.Queries;

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