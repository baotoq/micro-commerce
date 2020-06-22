using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Common;
using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Catalog.API.Application.Categories.Queries
{
    public class FindCategoriesQuery : IRequest<CursorPaged<CategoryDto>>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class FindCategoriesQueryHandler : IRequestHandler<FindCategoriesQuery, CursorPaged<CategoryDto>>
    {
        private readonly IRepository<Category> _repository;

        public FindCategoriesQueryHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<CursorPaged<CategoryDto>> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Select(s => new CategoryDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .OrderBy(s => s.Id)
                .Where(s => s.Id >= request.Page)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var next = await _repository.Query()
                .Select(s => new { s.Id })
                .OrderBy(s => s.Id)
                .Where(s => s.Id >= request.Page)
                .Skip(request.PageSize)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var previous = await _repository.Query()
                .Select(s => new { s.Id })
                .OrderByDescending(s => s.Id)
                .Where(s => s.Id < request.Page)
                .Skip(request.PageSize - 1)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            var paged = new CursorPaged<CategoryDto>(result.ToList())
            {
                PreviousPage = previous?.Id ?? -1,
                NextPage = next?.Id ?? -1,
            };

            return paged;
        }
    }
}
