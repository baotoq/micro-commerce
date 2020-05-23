using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Categories.Models;
using BShop.API.Data;
using BShop.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Application.Categories.Queries.GetAll
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
    {
        private readonly IRepository<Category> _repository;

        public GetAllCategoriesQueryHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Select(x => new CategoryDto { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
