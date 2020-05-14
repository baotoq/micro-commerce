using BShop.API.Features.Categories.Models;
using MediatR;

namespace BShop.API.Features.Categories.Queries.GetById
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
