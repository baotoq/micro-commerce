using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.API.Infrastructure
{
    public abstract class NoTrackingQueryHandler
    {
        public NoTrackingQueryHandler(DbContext context)
        {
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}
