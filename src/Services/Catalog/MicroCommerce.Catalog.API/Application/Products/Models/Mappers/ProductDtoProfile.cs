using AutoMapper;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Persistence.Entities;

namespace MicroCommerce.Catalog.API.Application.Products.Models.Mappers
{
    public class ProductDtoProfile : Profile
    {
        public ProductDtoProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<OffsetPaged<Product>, OffsetPaged<ProductDto>>();
        }
    }
}
