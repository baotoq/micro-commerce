using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartProductMaps");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DeliveryOptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Buyers");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Promotions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedDiscount",
                table: "Promotions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumDiscount",
                table: "Promotions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumSpending",
                table: "Promotions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Promotions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryOptionId",
                table: "Carts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PromotionId",
                table: "Carts",
                type: "character varying(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "Carts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCheckoutAmount",
                table: "Carts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPromotionDiscountAmount",
                table: "Carts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ProductQuantity = table.Column<int>(type: "integer", nullable: false),
                    ProductPriceAtCheckoutTime = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => new { x.CartId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_DeliveryOptionId",
                table: "Carts",
                column: "DeliveryOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_PromotionId",
                table: "Carts",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_DeliveryOptions_DeliveryOptionId",
                table: "Carts",
                column: "DeliveryOptionId",
                principalTable: "DeliveryOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Promotions_PromotionId",
                table: "Carts",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_DeliveryOptions_DeliveryOptionId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Promotions_PromotionId",
                table: "Carts");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Carts_DeliveryOptionId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_PromotionId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "FixedDiscount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaximumDiscount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinimumSpending",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeliveryOptionId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PromotionId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "TotalCheckoutAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "TotalPromotionDiscountAmount",
                table: "Carts");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Sellers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Promotions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "DeliveryOptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Carts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Buyers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CartProductMaps",
                columns: table => new
                {
                    CartId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ProductQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartProductMaps", x => new { x.CartId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_CartProductMaps_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartProductMaps_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartProductMaps_ProductId",
                table: "CartProductMaps",
                column: "ProductId");
        }
    }
}
