using AutoMapper;
using MicroCommerce.Catalog.API.Persistence.Entities;

namespace MicroCommerce.Catalog.API.Application.Categories.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<Category, CategoryDto>();
            }
        }
    }
}
