using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Accounts.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "accounts",
                table: "Merchants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "accounts",
                table: "Merchants");
        }
    }
}
