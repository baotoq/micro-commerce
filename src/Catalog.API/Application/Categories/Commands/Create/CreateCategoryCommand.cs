using Catalog.API.Application.Categories.Models;
using MediatR;

namespace Catalog.API.Application.Categories.Commands.Create
{
    public class CreateCategoryCommand : IRequest<CategoryDto>
    {
        public string? Name { get; set; }
    }
}
