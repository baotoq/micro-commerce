using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Identity.API.Application.Roles.Queries
{
    public class FindRolesQuery : IRequest<List<RoleDto>>
    {
    }

    public class FindRolesQueryHandler : IRequestHandler<FindRolesQuery, List<RoleDto>>
    {
        private readonly IRepository<Role, string> _repository;

        public FindRolesQueryHandler(IRepository<Role, string> repository)
        {
            _repository = repository;
        }

        public async Task<List<RoleDto>> Handle(FindRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Select(s => new RoleDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
