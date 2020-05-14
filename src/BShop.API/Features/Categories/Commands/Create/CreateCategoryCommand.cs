using BShop.API.Features.Categories.Models;
using MediatR;

namespace BShop.API.Features.Categories.Commands.Create
{
    public class CreateCategoryCommand : IRequest<CategoryDto>
    {
        public string Name { get; set; }
    }
}
