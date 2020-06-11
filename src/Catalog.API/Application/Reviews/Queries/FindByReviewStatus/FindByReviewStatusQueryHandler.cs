using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Catalog.API.Application.Reviews.Queries.FindByReviewStatus
{
    public class FindByReviewStatusQueryHandler : IRequestHandler<FindByReviewStatusQuery, List<Review>>
    {
        private readonly IRepository<Review> _repository;

        public FindByReviewStatusQueryHandler(IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<List<Review>> Handle(FindByReviewStatusQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Where(s => s.ReviewStatus == request.ReviewStatus)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
