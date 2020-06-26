using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalog.API.Data.Migrations
{
    public partial class UpdateProductQuanity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartMaxQuantity",
                table: "Products",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SellQuantity",
                table: "Products",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartMaxQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellQuantity",
                table: "Products");
        }
    }
}
