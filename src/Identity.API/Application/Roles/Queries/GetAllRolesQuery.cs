using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Identity.API.Application.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<List<Role>>
    {
    }

    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<Role>>
    {
        private readonly IRepository<Role> _repository;

        public GetAllRolesQueryHandler(IRepository<Role> repository)
        {
            _repository = repository;
        }

        public async Task<List<Role>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query().ToListAsync(cancellationToken);

            return result;
        }
    }
}
