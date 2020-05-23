using System.Collections.Generic;
using BShop.API.Application.Categories.Models;
using MediatR;

namespace BShop.API.Application.Categories.Queries.GetAll
{
    public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
    {
    }
}
