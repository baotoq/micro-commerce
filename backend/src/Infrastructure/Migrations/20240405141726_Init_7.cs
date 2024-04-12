using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddressId",
                table: "Carts",
                type: "character varying(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Carts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DeliveryAddresses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecipientPhoneNumber = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeliveryInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_DeliveryAddressId",
                table: "Carts",
                column: "DeliveryAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_DeliveryAddresses_DeliveryAddressId",
                table: "Carts",
                column: "DeliveryAddressId",
                principalTable: "DeliveryAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_DeliveryAddresses_DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropTable(
                name: "DeliveryAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Carts_DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Carts");
        }
    }
}
