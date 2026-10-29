using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                schema: "catalog",
                columns: table => new
                {
                    CheckoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HoldUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => new { x.CheckoutId, x.VariantId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ProductId",
                schema: "catalog",
                table: "Reservations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VariantId",
                schema: "catalog",
                table: "Reservations",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations",
                schema: "catalog");
        }
    }
}
