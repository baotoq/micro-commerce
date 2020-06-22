using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalog.API.Data.Migrations
{
    public partial class UpdateProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RatingAverage",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Products");
        }
    }
}
