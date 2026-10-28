using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Accounts.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantProfileVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileVersion",
                schema: "accounts",
                table: "Merchants",
                type: "integer",
                nullable: false,
                // Existing Merchants are at their first profile version, as new ones start.
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileVersion",
                schema: "accounts",
                table: "Merchants");
        }
    }
}
