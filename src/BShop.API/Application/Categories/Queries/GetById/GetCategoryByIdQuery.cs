using BShop.API.Application.Categories.Models;
using MediatR;

namespace BShop.API.Application.Categories.Queries.GetById
{
    public class GetCategoryByIdQuery : IRequest<CategoryDto>
    {
        public long Id { get; set; }

        public GetCategoryByIdQuery(long id)
        {
            Id = id;
        }
    }
}
