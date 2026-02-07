using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                schema: "ordering",
                table: "Orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                schema: "ordering",
                table: "Orders");
        }
    }
}
