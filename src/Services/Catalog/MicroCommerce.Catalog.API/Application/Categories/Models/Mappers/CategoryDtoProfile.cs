using AutoMapper;
using MicroCommerce.Catalog.API.Persistence.Entities;

namespace MicroCommerce.Catalog.API.Application.Categories.Models.Mappers
{
    public class CategoryDtoProfile : Profile
    {
        public CategoryDtoProfile()
        {
            CreateMap<Category, CategoryDto>();
        }
    }
}
