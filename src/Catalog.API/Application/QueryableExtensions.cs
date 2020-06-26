using System.Linq;
using Catalog.API.Data.Models;

namespace Catalog.API.Application
{
    public static class QueryableExtensions
    {
        public static IQueryable<Cart> FindActiveCart(this IQueryable<Cart> queryable, long customerId)
        {
            return queryable.Where(s => s.CustomerId == customerId && s.IsActive);
        }
    }
}
