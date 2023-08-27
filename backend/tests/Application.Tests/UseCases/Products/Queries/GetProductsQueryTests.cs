using Application.Common.AutoMapper;
using Application.UseCases.Carts.Queries;
using Application.UseCases.Products;
using Application.UseCases.Products.Queries;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.UseCases.Products.Queries;

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