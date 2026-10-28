using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "OptionValues",
                schema: "catalog",
                table: "Variants",
                type: "text[]",
                nullable: false,
                // Existing Products have a single default Variant, which has no option values.
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<int>(
                name: "Position",
                schema: "catalog",
                table: "Variants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Options",
                schema: "catalog",
                table: "Products",
                type: "jsonb",
                nullable: false,
                // ... and no options.
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionValues",
                schema: "catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Position",
                schema: "catalog",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "Options",
                schema: "catalog",
                table: "Products");
        }
    }
}
