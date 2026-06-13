using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductConcurrencyToken : Migration
    {
        // `xmin` is a PostgreSQL system column present on every table; it is mapped as an
        // optimistic-concurrency token in ProductConfiguration (review finding 2), not created.
        // The EF model differ still emits an AddColumn against the snapshot, but applying it
        // would collide with the reserved system column — so this migration is intentionally a
        // no-op. It exists only to advance the model snapshot to record the token mapping.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
