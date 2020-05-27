using Microsoft.EntityFrameworkCore.Migrations;

namespace BShop.API.Data.Migrations
{
    public partial class UpdateImageUri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ImageUri",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUri",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
