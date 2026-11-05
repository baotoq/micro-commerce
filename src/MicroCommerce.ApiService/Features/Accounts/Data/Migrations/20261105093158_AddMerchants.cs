using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Accounts.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "accounts");

            migrationBuilder.CreateTable(
                name: "merchants",
                schema: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    normalized_shop_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    pickup_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pickup_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pickup_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchants_account_id",
                schema: "accounts",
                table: "merchants",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_merchants_normalized_shop_name",
                schema: "accounts",
                table: "merchants",
                column: "normalized_shop_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchants",
                schema: "accounts");
        }
    }
}
