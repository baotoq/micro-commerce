using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                schema: "cart",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Carts",
                schema: "cart",
                table: "Carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                schema: "cart",
                table: "CartItems");

            migrationBuilder.RenameTable(
                name: "Carts",
                schema: "cart",
                newName: "carts",
                newSchema: "cart");

            migrationBuilder.RenameTable(
                name: "CartItems",
                schema: "cart",
                newName: "cart_items",
                newSchema: "cart");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "cart",
                table: "carts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                schema: "cart",
                table: "carts",
                newName: "last_modified_at");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                schema: "cart",
                table: "carts",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "cart",
                table: "carts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                schema: "cart",
                table: "carts",
                newName: "buyer_id");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_ExpiresAt",
                schema: "cart",
                table: "carts",
                newName: "ix_carts_expires_at");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_BuyerId",
                schema: "cart",
                table: "carts",
                newName: "ix_carts_buyer_id");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "cart",
                table: "cart_items",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "cart",
                table: "cart_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                schema: "cart",
                table: "cart_items",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                schema: "cart",
                table: "cart_items",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "cart",
                table: "cart_items",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "cart",
                table: "cart_items",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CartId",
                schema: "cart",
                table: "cart_items",
                newName: "cart_id");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_CartId",
                schema: "cart",
                table: "cart_items",
                newName: "ix_cart_items_cart_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_carts",
                schema: "cart",
                table: "carts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cart_items",
                schema: "cart",
                table: "cart_items",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_carts_cart_id",
                schema: "cart",
                table: "cart_items",
                column: "cart_id",
                principalSchema: "cart",
                principalTable: "carts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_carts_cart_id",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_carts",
                schema: "cart",
                table: "carts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cart_items",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.RenameTable(
                name: "carts",
                schema: "cart",
                newName: "Carts",
                newSchema: "cart");

            migrationBuilder.RenameTable(
                name: "cart_items",
                schema: "cart",
                newName: "CartItems",
                newSchema: "cart");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "cart",
                table: "Carts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_at",
                schema: "cart",
                table: "Carts",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                schema: "cart",
                table: "Carts",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "cart",
                table: "Carts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "buyer_id",
                schema: "cart",
                table: "Carts",
                newName: "BuyerId");

            migrationBuilder.RenameIndex(
                name: "ix_carts_expires_at",
                schema: "cart",
                table: "Carts",
                newName: "IX_Carts_ExpiresAt");

            migrationBuilder.RenameIndex(
                name: "ix_carts_buyer_id",
                schema: "cart",
                table: "Carts",
                newName: "IX_Carts_BuyerId");

            migrationBuilder.RenameColumn(
                name: "quantity",
                schema: "cart",
                table: "CartItems",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "cart",
                table: "CartItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                schema: "cart",
                table: "CartItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "product_name",
                schema: "cart",
                table: "CartItems",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "cart",
                table: "CartItems",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "image_url",
                schema: "cart",
                table: "CartItems",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "cart_id",
                schema: "cart",
                table: "CartItems",
                newName: "CartId");

            migrationBuilder.RenameIndex(
                name: "ix_cart_items_cart_id",
                schema: "cart",
                table: "CartItems",
                newName: "IX_CartItems_CartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carts",
                schema: "cart",
                table: "Carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                schema: "cart",
                table: "CartItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                schema: "cart",
                table: "CartItems",
                column: "CartId",
                principalSchema: "cart",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
