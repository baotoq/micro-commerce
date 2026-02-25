using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditableAggregateRootAdoption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_modified_at",
                schema: "cart",
                table: "carts",
                newName: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "cart",
                table: "carts",
                newName: "last_modified_at");
        }
    }
}
