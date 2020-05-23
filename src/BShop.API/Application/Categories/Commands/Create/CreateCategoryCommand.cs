using BShop.API.Application.Categories.Models;
using MediatR;

namespace BShop.API.Application.Categories.Commands.Create
{
    public class CreateCategoryCommand : IRequest<CategoryDto>
    {
        public string? Name { get; set; }
    }
}
