using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.API.Application.Roles.Queries;
using Identity.API.Data.Models;
using IdentityServer4;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.ApiControllers
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllRolesQuery(), cancellationToken);

            return result;
        }
    }
}
