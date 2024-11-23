using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.MigrationService.Migrations
{
    /// <inheritdoc />
    public partial class Update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Buyers_BuyerId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_DeliveryAddresses_DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_DeliveryOptions_DeliveryOptionId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Promotions_PromotionId",
                table: "Carts");

            migrationBuilder.AlterColumn<Guid>(
                name: "PromotionId",
                table: "Carts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryOptionId",
                table: "Carts",
                type: "uuid",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryAddressId",
                table: "Carts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "BuyerId",
                table: "Carts",
                type: "uuid",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Buyers_BuyerId",
                table: "Carts",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_DeliveryAddresses_DeliveryAddressId",
                table: "Carts",
                column: "DeliveryAddressId",
                principalTable: "DeliveryAddresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_DeliveryOptions_DeliveryOptionId",
                table: "Carts",
                column: "DeliveryOptionId",
                principalTable: "DeliveryOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Promotions_PromotionId",
                table: "Carts",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Buyers_BuyerId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_DeliveryAddresses_DeliveryAddressId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_DeliveryOptions_DeliveryOptionId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Promotions_PromotionId",
                table: "Carts");

            migrationBuilder.AlterColumn<Guid>(
                name: "PromotionId",
                table: "Carts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryOptionId",
                table: "Carts",
                type: "uuid",
                maxLength: 100,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryAddressId",
                table: "Carts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BuyerId",
                table: "Carts",
                type: "uuid",
                maxLength: 100,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Buyers_BuyerId",
                table: "Carts",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_DeliveryAddresses_DeliveryAddressId",
                table: "Carts",
                column: "DeliveryAddressId",
                principalTable: "DeliveryAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
