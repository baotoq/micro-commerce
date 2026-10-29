using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Ordering.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Checkouts",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PlacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveryAddress_RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeliveryAddress_Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DeliveryAddress_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeliveryAddress_Ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeliveryAddress_Province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemsTotal = table.Column<long>(type: "bigint", nullable: false),
                    ShippingTotal = table.Column<long>(type: "bigint", nullable: false),
                    Total = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckoutMerchants",
                schema: "ordering",
                columns: table => new
                {
                    CheckoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subtotal = table.Column<long>(type: "bigint", nullable: false),
                    ShippingFee = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutMerchants", x => new { x.CheckoutId, x.MerchantId });
                    table.ForeignKey(
                        name: "FK_CheckoutMerchants_Checkouts_CheckoutId",
                        column: x => x.CheckoutId,
                        principalSchema: "ordering",
                        principalTable: "Checkouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                schema: "ordering",
                columns: table => new
                {
                    CheckoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OptionValues = table.Column<string[]>(type: "text[]", nullable: false),
                    UnitPrice = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => new { x.CheckoutId, x.VariantId });
                    table.ForeignKey(
                        name: "FK_OrderLines_Checkouts_CheckoutId",
                        column: x => x.CheckoutId,
                        principalSchema: "ordering",
                        principalTable: "Checkouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checkouts_AccountId",
                schema: "ordering",
                table: "Checkouts",
                column: "AccountId",
                unique: true,
                filter: "\"Status\" = 'AwaitingPayment'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoutMerchants",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "OrderLines",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "Checkouts",
                schema: "ordering");
        }
    }
}
