using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalog.API.Data.Migrations
{
    public partial class UpdateStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reply");

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Review",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReplyStatus",
                table: "Reply",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ReplyStatus",
                table: "Reply");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reply",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
