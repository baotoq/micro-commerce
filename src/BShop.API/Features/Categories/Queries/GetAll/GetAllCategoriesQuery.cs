using System.Collections.Generic;
using BShop.API.Features.Categories.Models;
using MediatR;

namespace BShop.API.Features.Categories.Queries.GetAll
{
    public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
    {
    }
}
