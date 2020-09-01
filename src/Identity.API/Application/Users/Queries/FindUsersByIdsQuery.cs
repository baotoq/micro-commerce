using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.UnitOfWork;
using Identity.API.Application.Users.Models;
using Identity.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Application.Users.Queries
{
    public class FindUsersByIdsQuery : IRequest<List<UserDto>>
    {
        public IList<string> Ids { get; set; }
    }

    public class FindUsersByIdsQueryHandler : IRequestHandler<FindUsersByIdsQuery, List<UserDto>>
    {
        private readonly IRepository<User, string> _repository;

        public FindUsersByIdsQueryHandler(IRepository<User, string> repository)
        {
            _repository = repository;
        }

        public async Task<List<UserDto>> Handle(FindUsersByIdsQuery request, CancellationToken cancellationToken)
        {
            var users = await _repository.Query()
                .Where(s => request.Ids.Contains(s.Id))
                .Select(s => new UserDto
                {
                    Id = s.Id,
                    Email = s.Email,
                    UserName = s.UserName,
                    RoleId = s.Roles.FirstOrDefault().RoleId
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
