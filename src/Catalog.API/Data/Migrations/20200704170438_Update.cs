using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalog.API.Data.Migrations
{
    public partial class Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNote",
                table: "Carts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNote",
                table: "Carts");
        }
    }
}
