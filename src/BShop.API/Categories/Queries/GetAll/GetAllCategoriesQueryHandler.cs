using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Categories.Models;
using BShop.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Categories.Queries.GetAll
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllCategoriesQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Categories
                .Select(x => new CategoryDto { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
