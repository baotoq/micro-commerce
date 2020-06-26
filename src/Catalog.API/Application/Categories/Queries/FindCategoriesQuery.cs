using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Models;
using UnitOfWork;
using UnitOfWork.Common;

namespace Catalog.API.Application.Categories.Queries
{
    public class FindCategoriesQuery : CursorPagedQuery<long>, IRequest<CursorPaged<CategoryDto, long?>>
    {
    }

    public class FindCategoriesQueryHandler : IRequestHandler<FindCategoriesQuery, CursorPaged<CategoryDto, long?>>
    {
        private readonly IRepository<Category> _repository;

        public FindCategoriesQueryHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<CursorPaged<CategoryDto, long?>> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .OrderBy(s => s.Id)
                .Where(s => s.Id >= request.PageToken)
                .Take(request.PageSize)
                .Select(s => new CategoryDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync(cancellationToken);

            var next = await _repository.Query()
                .OrderBy(s => s.Id)
                .Where(s => s.Id >= request.PageToken)
                .Skip(request.PageSize)
                .Select(s => new { s.Id })
                .FirstOrDefaultAsync(cancellationToken);

            var previous = await _repository.Query()
                .OrderByDescending(s => s.Id)
                .Where(s => s.Id < request.PageToken)
                .Skip(request.PageSize - 1)
                .Select(s => new { s.Id })
                .FirstOrDefaultAsync(cancellationToken);

            var paged = new CursorPaged<CategoryDto, long?>()
            {
                Data = result.ToList(),
                PreviousPageToken = previous?.Id,
                NextPageToken = next?.Id,
            };

            return paged;
        }
    }
}
