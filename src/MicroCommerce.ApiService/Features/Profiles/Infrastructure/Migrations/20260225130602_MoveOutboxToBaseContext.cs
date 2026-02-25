using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Profiles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveOutboxToBaseContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: outbox tables created by CatalogDbContext migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: outbox tables owned by CatalogDbContext migration
        }
    }
}
