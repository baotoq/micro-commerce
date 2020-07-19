using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Data.Models;
using MediatR;
using Shared.MediatR.Models;
using UnitOfWork;
using UnitOfWork.Common;

namespace Catalog.API.Application.Categories.Queries
{
    public class FindCategoriesQuery : OffsetPagedQuery, IRequest<OffsetPaged<CategoryDto>>
    {
        public string QueryString { get; set; }
    }

    public class FindCategoriesQueryHandler : IRequestHandler<FindCategoriesQuery, OffsetPaged<CategoryDto>>
    {
        private readonly IRepository<Category> _repository;

        public FindCategoriesQueryHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<OffsetPaged<CategoryDto>> Handle(FindCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Query();

            if (!string.IsNullOrEmpty(request.QueryString))
            {
                request.QueryString = request.QueryString.ToUpperInvariant();
                query = query.Where(s => s.Name.ToUpperInvariant().Contains(request.QueryString));
            }

            var result = await query
                .Select(s => new CategoryDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            return result;
        }
    }
}
