using BShop.API.Categories.Models;
using MediatR;

namespace BShop.API.Categories.Queries.GetById
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
