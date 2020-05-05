using System.Collections.Generic;
using BShop.API.Categories.Models;
using MediatR;

namespace BShop.API.Categories.Queries.GetAll
{
    public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
    {
    }
}
