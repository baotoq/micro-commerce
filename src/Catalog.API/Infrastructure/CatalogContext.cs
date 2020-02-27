using Catalog.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure
{
    public class CatalogContext : DbContext
    {
        public DbSet<CatalogItem> CatalogItems { get; set; } = null!;

        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
        }
    }
}
