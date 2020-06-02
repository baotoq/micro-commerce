using System.Collections.Generic;
using Catalog.API.Application.Categories.Models;
using MediatR;

namespace Catalog.API.Application.Categories.Queries.GetAll
{
    public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
    {
    }
}
