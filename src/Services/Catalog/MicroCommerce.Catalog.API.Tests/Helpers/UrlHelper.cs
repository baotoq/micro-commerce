using System.Linq;

namespace MicroCommerce.Catalog.API.Tests.Helpers
{
    public static class UrlHelper
    {
        public static string Combine(string url, params string[] paths)
        {
            return new(paths.Aggregate(url, (current, path) => $"{current.TrimEnd('/')}/{path.TrimStart('/')}"));
        }
    }
}
