using BShop.API.Categories.Models;
using MediatR;

namespace BShop.API.Categories.Queries.GetById
{
    public class GetCategoryByIdQuery : IRequest<CategoryDto>
    {
        public int Id { get; set; }

        public GetCategoryByIdQuery(int id)
        {
            Id = id;
        }
    }
}
