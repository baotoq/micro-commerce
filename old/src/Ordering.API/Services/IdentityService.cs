using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Ordering.API.Services
{
    public interface IIdentityService
    {
        string GetCurrentUserId();
    }

    public class IdentityService : IIdentityService
    {
        protected readonly HttpContext HttpContext;

        public IdentityService(IHttpContextAccessor context)
        {
            HttpContext = context.HttpContext;
        }

        public string GetCurrentUserId()
        {
            return HttpContext.User.FindFirstValue("sub");
        }
    }
}
