using AutoMapper;
using MicroCommerce.Catalog.API.Application.Categories.Models.Mappers;
using MicroCommerce.Catalog.API.Application.Products.Models.Mappers;
using Xunit;

namespace MicroCommerce.Catalog.API.Tests
{
    public class CategoryDtoMapperTests
    {
        private readonly MapperConfiguration _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CategoryDtoProfile>();
            cfg.AddProfile<ProductDtoProfile>();
        });

        [Fact]
        public void AssertConfigurationIsValid()
        {
            _configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public void CategoryDtoProfile()
        {
            _configuration.AssertConfigurationIsValid<CategoryDtoProfile>();
        }

        [Fact]
        public void ProductDtoProfile()
        {
            _configuration.AssertConfigurationIsValid<ProductDtoProfile>();
        }
    }
}
