using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Identity.API.Application.Roles.Queries
{
    public class GetAllUsersQuery : IRequest<List<User>>
    {
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<User>>
    {
        private readonly IRepository<User> _repository;

        public GetAllUsersQueryHandler(IRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query().ToListAsync(cancellationToken);

            return result;
        }
    }
}
