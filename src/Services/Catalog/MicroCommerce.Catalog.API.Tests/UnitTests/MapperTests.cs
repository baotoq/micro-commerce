using AutoMapper;
using MicroCommerce.Catalog.API.Application.Categories.Models;
using MicroCommerce.Catalog.API.Application.Products.Commands;
using MicroCommerce.Catalog.API.Application.Products.Models;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests.UnitTests
{
    public class CategoryDtoMapperTests
    {
        private readonly MapperConfiguration _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CategoryDto.MapperProfile>();
            cfg.AddProfile<ProductDto.MapperProfile>();
            cfg.AddProfile<CreateProductCommand.MapperProfile>();
        });

        //[Fact]
        //public void AssertConfigurationIsValid()
        //{
        //    _configuration.AssertConfigurationIsValid();
        //}

        [Fact]
        public void CategoryDto()
        {
            _configuration.AssertConfigurationIsValid<CategoryDto.MapperProfile>();
        }

        [Fact]
        public void ProductDto()
        {
            _configuration.AssertConfigurationIsValid<ProductDto.MapperProfile>();
        }

        //[Fact]
        //public void CreateProductCommand()
        //{
        //    _configuration.AssertConfigurationIsValid<CreateProductCommand.MapperProfile>();
        //}
    }
}
