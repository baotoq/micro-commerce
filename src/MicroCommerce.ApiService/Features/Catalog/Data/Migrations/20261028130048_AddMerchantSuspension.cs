using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantSuspension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                schema: "catalog",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuspended",
                schema: "catalog",
                table: "Shops");
        }
    }
}
