using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Wishlists.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WishlistItems",
                schema: "wishlists",
                table: "WishlistItems");

            migrationBuilder.RenameTable(
                name: "WishlistItems",
                schema: "wishlists",
                newName: "wishlist_items",
                newSchema: "wishlists");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "AddedAt",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "added_at");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_UserId_ProductId",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "ix_wishlist_items_user_id_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_UserId",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "ix_wishlist_items_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_AddedAt",
                schema: "wishlists",
                table: "wishlist_items",
                newName: "ix_wishlist_items_added_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_wishlist_items",
                schema: "wishlists",
                table: "wishlist_items",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_wishlist_items",
                schema: "wishlists",
                table: "wishlist_items");

            migrationBuilder.RenameTable(
                name: "wishlist_items",
                schema: "wishlists",
                newName: "WishlistItems",
                newSchema: "wishlists");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "added_at",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "AddedAt");

            migrationBuilder.RenameIndex(
                name: "ix_wishlist_items_user_id_product_id",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "IX_WishlistItems_UserId_ProductId");

            migrationBuilder.RenameIndex(
                name: "ix_wishlist_items_user_id",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "IX_WishlistItems_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_wishlist_items_added_at",
                schema: "wishlists",
                table: "WishlistItems",
                newName: "IX_WishlistItems_AddedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WishlistItems",
                schema: "wishlists",
                table: "WishlistItems",
                column: "Id");
        }
    }
}
