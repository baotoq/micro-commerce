using BShop.API.Categories.Models;
using MediatR;

namespace BShop.API.Categories.Commands.Create
{
    public class CreateCategoryCommand : IRequest<CategoryDto>
    {
        public string Name { get; set; }
    }
}
