using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Catalog.API.Services
{
    public interface IIdentityService
    {
        long GetCurrentUserId();
    }

    public class IdentityService : IIdentityService
    {
        protected readonly HttpContext HttpContext;

        public IdentityService(IHttpContextAccessor context)
        {
            HttpContext = context.HttpContext;
        }

        public long GetCurrentUserId()
        {
            return long.Parse(HttpContext.User.FindFirstValue("sub"));
        }
    }
}
